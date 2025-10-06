using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Interface;

public interface IGoogleOAuthService
{
    Task<GoogleOAuthUser> ExchangeCodeAsync(string authorizationCode, string redirectUri, string codeVerifier, CancellationToken ct);
    Task<GoogleOAuthUser> ValidateIdTokenAsync(string idToken, CancellationToken ct);
}

public sealed record GoogleOAuthUser(
    string Subject,
    string Email,
    bool EmailVerified,
    string? FullName,
    string? PictureUrl);
