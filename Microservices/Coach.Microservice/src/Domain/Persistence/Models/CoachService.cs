namespace Domain.Persistence.Models;

public partial class CoachService
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public int SessionsTotal { get; set; }

    public long PriceVnd { get; set; }

    public bool OnlineAvailable { get; set; }

    public bool OnsiteAvailable { get; set; }

    public string? LocationNote { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid SportId { get; set; }

    public virtual CoachProfile Coach { get; set; } = null!;

    public virtual Sport Sport { get; set; } = null!;
}
