namespace Domain.Entities;

public class ExternalLogin
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Provider { get; set; } = null!;

    public string ProviderUserId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual AppUser User { get; set; } = null!;
}