namespace Application.Abstractions.Interface;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}