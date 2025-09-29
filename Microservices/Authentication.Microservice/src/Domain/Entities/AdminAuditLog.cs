namespace Domain.Entities;

public partial class AdminAuditLog
{
    public Guid Id { get; set; }

    public Guid AdminId { get; set; }

    public string Action { get; set; } = null!;

    public string TargetType { get; set; } = null!;

    public Guid? TargetId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppUser Admin { get; set; } = null!;
}
