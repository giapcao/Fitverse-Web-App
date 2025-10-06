namespace Domain.Entities;

public partial class Role
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
