namespace Application.Abstractions.Interface;

public interface IRefreshTokenStore
{
    Task<string> IssueAsync(Guid userId, CancellationToken ct);
    Task<(bool ok, string? current)> ValidateAsync(Guid userId, string incoming, CancellationToken ct);
    Task RevokeAsync(Guid userId, CancellationToken ct);
}