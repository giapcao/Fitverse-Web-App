using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Persistence.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence.Models;

[Table("timeslot")]
[Index(nameof(CoachId), nameof(StartAt), Name = "idx_timeslot_user_time")]
[Index(nameof(CoachId), nameof(StartAt), nameof(EndAt), IsUnique = true, Name = "timeslot_coach_id_start_at_end_at_key")]
[Index(nameof(Status), Name = "idx_timeslot_status")]
public class Timeslot
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

    [Column("status")]
    public SlotStatus Status { get; set; } = SlotStatus.Open;

    [Column("is_online")]
    public bool IsOnline { get; set; } = true;

    [Column("onsite_lat")]
    public double? OnsiteLat { get; set; }

    [Column("onsite_lng")]
    public double? OnsiteLng { get; set; }

    [Column("capacity")]
    public int Capacity { get; set; } = 1;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty(nameof(Booking.Timeslot))]
    public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();

    [InverseProperty(nameof(SubscriptionEvent.Timeslot))]
    public virtual ICollection<SubscriptionEvent> SubscriptionEvents { get; set; } = new HashSet<SubscriptionEvent>();
}
