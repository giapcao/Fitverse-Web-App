namespace Application.Abstractions.Interface;

public interface IOtpStore
{
    Task<bool> CanIssueAsync(string email, CancellationToken ct);
    Task<string> IssueAsync(string email, Guid userId, TimeSpan ttl, CancellationToken ct);
    Task<(bool isValid, Guid userId)> VerifyAndConsumeAsync(string email, string otp, int maxAttempts, CancellationToken ct);
}