namespace Domain.Entities;

public partial class Role
{
    public string Id { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
