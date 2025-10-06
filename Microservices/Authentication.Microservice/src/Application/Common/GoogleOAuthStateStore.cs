using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Interface;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Options = Infrastructure.Common.Options;

namespace Application.Common;

public sealed class GoogleOAuthStateStore : IGoogleOAuthStateStore
{
    private const string KeyPrefix = "google:oauth:state:";
    private readonly IConnectionMultiplexer _redis;
    private readonly Options.GoogleOAuthOptions _options;

    public GoogleOAuthStateStore(IConnectionMultiplexer redis, IOptions<Options.GoogleOAuthOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    public async Task<GoogleOAuthStateIssueResult> IssueAsync(string? redirectUri, CancellationToken ct)
         {
             var resolvedRedirect = string.IsNullOrWhiteSpace(redirectUri) ? _options.RedirectUri : redirectUri;
             if (string.IsNullOrWhiteSpace(resolvedRedirect))
             {
                 throw new InvalidOperationException("Google redirect URI is not configured.");
             }
     
             var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
             var codeVerifier = GenerateCodeVerifier(Math.Max(32, _options.CodeVerifierLength));
             var codeChallenge = Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));
     
             var payload = JsonSerializer.Serialize(new StatePayload
             {
                 RedirectUri = resolvedRedirect,
                 CodeVerifier = codeVerifier
             });
     
             var ttl = TimeSpan.FromMinutes(Math.Clamp(_options.StateTtlMinutes, 1, 30));
             var db = _redis.GetDatabase();
             await db.StringSetAsync(KeyPrefix + state, payload, ttl);
     
             return new GoogleOAuthStateIssueResult(state, codeChallenge, "S256", resolvedRedirect);
         }

    public async Task<GoogleOAuthStateData?> RedeemAsync(string state, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return null;
        }

        var db = _redis.GetDatabase();
        var key = KeyPrefix + state;
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        await db.KeyDeleteAsync(key);

        var payload = JsonSerializer.Deserialize<StatePayload>(value!);
        if (payload is null || string.IsNullOrWhiteSpace(payload.CodeVerifier) || string.IsNullOrWhiteSpace(payload.RedirectUri))
        {
            return null;
        }

        return new GoogleOAuthStateData(payload.RedirectUri, payload.CodeVerifier);
    }

    private static string GenerateCodeVerifier(int length)
    {
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes.ToArray());
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class StatePayload
    {
        public string RedirectUri { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;
    }
}