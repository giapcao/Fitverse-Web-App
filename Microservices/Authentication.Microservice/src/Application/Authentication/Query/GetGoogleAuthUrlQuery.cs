using System.Text;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Features;
using Options = Infrastructure.Common.Options;
using SharedLibrary.Common.ResponseModel;
using Microsoft.Extensions.Options;

namespace Application.Authentication.Query;

public sealed record GetGoogleAuthUrlQuery(string? RedirectUri) : IQuery<AuthDto.GoogleAuthUrlDto>;

public sealed class GetGoogleAuthUrlQueryHandler : IQueryHandler<GetGoogleAuthUrlQuery, AuthDto.GoogleAuthUrlDto>
{
    private readonly IGoogleOAuthStateStore _stateStore;
    private readonly Options.GoogleOAuthOptions _options;

    public GetGoogleAuthUrlQueryHandler(IGoogleOAuthStateStore stateStore, IOptions<Options.GoogleOAuthOptions> options)
    {
        _stateStore = stateStore;
        _options = options.Value;
    }

    public async Task<Result<AuthDto.GoogleAuthUrlDto>> Handle(GetGoogleAuthUrlQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            return Result.Failure<AuthDto.GoogleAuthUrlDto>(new Error("google.config.missing", "Google OAuth client id is not configured."));
        }

        var resolvedRedirect = string.IsNullOrWhiteSpace(request.RedirectUri)
            ? _options.RedirectUri
            : request.RedirectUri;

        if (string.IsNullOrWhiteSpace(resolvedRedirect))
        {
            return Result.Failure<AuthDto.GoogleAuthUrlDto>(new Error("google.redirect.missing", "Redirect URI is required."));
        }

        if (!Uri.TryCreate(resolvedRedirect, UriKind.Absolute, out _))
        {
            return Result.Failure<AuthDto.GoogleAuthUrlDto>(new Error("google.redirect.invalid", "Redirect URI must be absolute."));
        }

        var state = await _stateStore.IssueAsync(resolvedRedirect, ct);

        var scopes = (_options.Scopes is { Length: > 0 } ? string.Join(" ", _options.Scopes) : "openid profile email").Trim();
        var authorizationEndpoint = string.IsNullOrWhiteSpace(_options.AuthorizationUri)
            ? "https://accounts.google.com/o/oauth2/v2/auth"
            : _options.AuthorizationUri;

        var query = new (string Key, string Value)[]
        {
            ("client_id", _options.ClientId),
            ("redirect_uri", resolvedRedirect),
            ("response_type", "code"),
            ("scope", scopes),
            ("state", state.State),
            ("code_challenge", state.CodeChallenge),
            ("code_challenge_method", state.CodeChallengeMethod),
            ("access_type", "offline"),
            ("include_granted_scopes", "true"),
            ("prompt", "consent")
        };

        var builder = new StringBuilder(authorizationEndpoint);
        var separator = authorizationEndpoint.Contains('?') ? '&' : '?';
        foreach (var (key, value) in query)
        {
            builder.Append(separator);
            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
            separator = '&';
        }

        return Result.Success(new AuthDto.GoogleAuthUrlDto(builder.ToString(), state.State));
    }
}
