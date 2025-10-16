using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence.Models;

[Table("coach_timeoff")]
[Index(nameof(CoachId), Name = "idx_timeoff_coach")]
[Index(nameof(CoachId), nameof(StartAt), nameof(EndAt), Name = "idx_timeoff_range")]
public class CoachTimeoff
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("coach_id")]
    public Guid CoachId { get; set; }

    [Column("start_at")]
    public DateTime StartAt { get; set; }

    [Column("end_at")]
    public DateTime EndAt { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
