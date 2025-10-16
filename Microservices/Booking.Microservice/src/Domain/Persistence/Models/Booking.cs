using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Persistence.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence.Models;

[Table("booking")]
[Index(nameof(CoachId), nameof(StartAt), Name = "idx_booking_coach")]
[Index(nameof(UserId), nameof(StartAt), Name = "idx_booking_user")]
[Index(nameof(Status), Name = "idx_booking_status")]
public class Booking
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("coach_id")]
    public Guid CoachId { get; set; }

    [Column("service_id")]
    public Guid ServiceId { get; set; }

    [Column("timeslot_id")]
    public Guid? TimeslotId { get; set; }

    [Column("start_at")]
    public DateTime StartAt { get; set; }

    [Column("end_at")]
    public DateTime EndAt { get; set; }

    [Column("status")]
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

    [Column("gross_amount_vnd")]
    public long GrossAmountVnd { get; set; }

    [Column("commission_pct")]
    [Precision(5, 2)]
    public decimal CommissionPct { get; set; } = 15.00m;

    [Column("commission_vnd")]
    public long CommissionVnd { get; set; }

    [Column("net_amount_vnd")]
    public long NetAmountVnd { get; set; }

    [Column("currency_code")]
    public string CurrencyCode { get; set; } = "VND";

    [Column("location_note")]
    public string? LocationNote { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("service_title")]
    public string? ServiceTitle { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [InverseProperty(nameof(SubscriptionEvent.Booking))]
    public virtual ICollection<SubscriptionEvent> SubscriptionEvents { get; set; } = new HashSet<SubscriptionEvent>();

    [ForeignKey(nameof(TimeslotId))]
    [InverseProperty(nameof(Models.Timeslot.Bookings))]
    public virtual Timeslot? Timeslot { get; set; }
}
