namespace Domain.Persistence.Models;

public partial class Sport
{
    public string DisplayName { get; set; } = null!;

    public Guid Id { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<CoachService> CoachServices { get; set; } = new List<CoachService>();

    public virtual ICollection<CoachProfile> Coaches { get; set; } = new List<CoachProfile>();
}
