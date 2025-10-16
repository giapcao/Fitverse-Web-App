using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence.Models;

[Table("availability_rule")]
[Index(nameof(CoachId), Name = "idx_availability_rule_coach")]
public class AvailabilityRule
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("coach_id")]
    public Guid CoachId { get; set; }

    [Column("weekday")]
    public int Weekday { get; set; }

    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time")]
    public TimeOnly EndTime { get; set; }

    [Column("slot_duration_minutes")]
    public int SlotDurationMinutes { get; set; } = 60;

    [Column("is_online")]
    public bool IsOnline { get; set; } = true;

    [Column("onsite_lat")]
    public double? OnsiteLat { get; set; }

    [Column("onsite_lng")]
    public double? OnsiteLng { get; set; }

    [Column("timezone")]
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
