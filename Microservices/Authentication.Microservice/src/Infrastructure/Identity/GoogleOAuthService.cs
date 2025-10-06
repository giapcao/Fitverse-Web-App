using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Interface;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options = Infrastructure.Common.Options;

namespace Infrastructure.Identity;

public sealed class GoogleOAuthService : IGoogleOAuthService
{
    private static readonly string[] ValidIssuers =
    {
        "accounts.google.com",
        "https://accounts.google.com"
    };

    private readonly HttpClient _client;
    private readonly Options.GoogleOAuthOptions _options;
    private readonly ILogger<GoogleOAuthService> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GoogleOAuthService(
        HttpClient client,
        IOptions<Options.GoogleOAuthOptions> options,
        ILogger<GoogleOAuthService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GoogleOAuthUser> ExchangeCodeAsync(string authorizationCode, string redirectUri, string codeVerifier, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            throw new ArgumentException("Authorization code is required.", nameof(authorizationCode));
        }

        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            throw new ArgumentException("Redirect URI is required.", nameof(redirectUri));
        }

        if (string.IsNullOrWhiteSpace(codeVerifier))
        {
            throw new ArgumentException("Code verifier is required.", nameof(codeVerifier));
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("Google OAuth client credentials are not configured.");
        }

        var tokenPayload = new Dictionary<string, string>
        {
            ["code"] = authorizationCode,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        };

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _options.TokenUri)
        {
            Content = new FormUrlEncodedContent(tokenPayload)
        };

        using var tokenResponse = await _client.SendAsync(tokenRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var body = await tokenResponse.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Google token exchange failed with status {StatusCode}: {Body}", (int)tokenResponse.StatusCode, body);
            throw new InvalidOperationException("Unable to verify Google credentials.");
        }

        await using var stream = await tokenResponse.Content.ReadAsStreamAsync(ct);
        var tokens = await JsonSerializer.DeserializeAsync<TokenResponse>(stream, _serializerOptions, ct);
        if (tokens is null || string.IsNullOrWhiteSpace(tokens.IdToken))
        {
            throw new InvalidOperationException("Google token exchange did not return a valid ID token.");
        }

        var payload = await ValidateIdTokenInternalAsync(tokens.IdToken);
        return await BuildUserAsync(payload, tokens.AccessToken, ct);
    }

    public async Task<GoogleOAuthUser> ValidateIdTokenAsync(string idToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new ArgumentException("ID token is required.", nameof(idToken));
        }

        var payload = await ValidateIdTokenInternalAsync(idToken);
        return await BuildUserAsync(payload, accessToken: null, ct);
    }

    private async Task<GoogleOAuthUser> BuildUserAsync(GoogleJsonWebSignature.Payload payload, string? accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new InvalidOperationException("Google account does not include a subject identifier.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new InvalidOperationException("Google account does not provide an email address.");
        }

        var email = payload.Email;
        var emailVerified = payload.EmailVerified == true;
        var fullName = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name;
        var picture = payload.Picture;

        if (!string.IsNullOrWhiteSpace(accessToken) && !string.IsNullOrWhiteSpace(_options.UserInfoUri))
        {
            try
            {
                using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoUri);
                userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var userInfoResponse = await _client.SendAsync(userInfoRequest, HttpCompletionOption.ResponseHeadersRead, ct);
                if (userInfoResponse.IsSuccessStatusCode)
                {
                    await using var userStream = await userInfoResponse.Content.ReadAsStreamAsync(ct);
                    var userInfo = await JsonSerializer.DeserializeAsync<UserInfoResponse>(userStream, _serializerOptions, ct);
                    if (userInfo is not null)
                    {
                        email = string.IsNullOrWhiteSpace(userInfo.Email) ? email : userInfo.Email;
                        fullName = string.IsNullOrWhiteSpace(userInfo.Name) ? fullName : userInfo.Name;
                        picture = string.IsNullOrWhiteSpace(userInfo.Picture) ? picture : userInfo.Picture;
                        if (userInfo.EmailVerified.HasValue)
                        {
                            emailVerified = userInfo.EmailVerified.Value;
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("Google userinfo request failed with status {StatusCode}", (int)userInfoResponse.StatusCode);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogDebug(ex, "Unable to fetch Google userinfo.");
            }
        }

        return new GoogleOAuthUser(payload.Subject, email, emailVerified, fullName, picture);
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateIdTokenInternalAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _options.ClientId }
                });

            if (!ValidIssuers.Contains(payload.Issuer))
            {
                throw new InvalidOperationException($"Unexpected token issuer '{payload.Issuer}'.");
            }

            return payload;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed");
            throw new InvalidOperationException("Google ID token validation failed.", ex);
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }

        [JsonPropertyName("id_token")] public string? IdToken { get; set; }

        [JsonPropertyName("token_type")] public string? TokenType { get; set; }

        [JsonPropertyName("scope")] public string? Scope { get; set; }
    }

    private sealed class UserInfoResponse
    {
        [JsonPropertyName("sub")] public string? Subject { get; set; }

        [JsonPropertyName("email")] public string? Email { get; set; }

        [JsonPropertyName("email_verified")] public bool? EmailVerified { get; set; }

        [JsonPropertyName("name")] public string? Name { get; set; }

        [JsonPropertyName("picture")] public string? Picture { get; set; }
    }
}