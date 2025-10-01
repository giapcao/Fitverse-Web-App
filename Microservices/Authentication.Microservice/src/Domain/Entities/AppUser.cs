namespace Domain.Entities;

public partial class AppUser
{
    public Guid Id { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Birth { get; set; }

    public string? Description { get; set; }

    public double? HomeLat { get; set; }

    public double? HomeLng { get; set; }

    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<AdminAuditLog> AdminAuditLogs { get; set; } = new List<AdminAuditLog>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
