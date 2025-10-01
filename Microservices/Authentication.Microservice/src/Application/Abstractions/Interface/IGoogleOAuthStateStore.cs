using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Interface;

public interface IGoogleOAuthStateStore
{
    Task<GoogleOAuthStateIssueResult> IssueAsync(string? redirectUri, CancellationToken ct);
    Task<GoogleOAuthStateData?> RedeemAsync(string state, CancellationToken ct);
}

public sealed record GoogleOAuthStateIssueResult(
    string State,
    string CodeChallenge,
    string CodeChallengeMethod,
    string RedirectUri);

public sealed record GoogleOAuthStateData(
    string RedirectUri,
    string CodeVerifier);
