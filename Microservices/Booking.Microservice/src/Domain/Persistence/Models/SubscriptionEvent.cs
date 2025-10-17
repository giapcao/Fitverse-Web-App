using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Persistence.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence.Models;

[Table("subscription_event")]
[Index(nameof(SubscriptionId), nameof(CreatedAt), Name = "idx_subscription_event_sub")]
[Index(nameof(EventType), Name = "idx_subscription_event_type")]
public class SubscriptionEvent
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("subscription_id")]
    public Guid SubscriptionId { get; set; }

    [Column("event_type")]
    public SubscriptionEventType EventType { get; set; }

    [Column("booking_id")]
    public Guid? BookingId { get; set; }

    [Column("timeslot_id")]
    public Guid? TimeslotId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BookingId))]
    [InverseProperty(nameof(Models.Booking.SubscriptionEvents))]
    public virtual Booking? Booking { get; set; }

    [ForeignKey(nameof(SubscriptionId))]
    [InverseProperty(nameof(Models.Subscription.SubscriptionEvents))]
    public virtual Subscription Subscription { get; set; } = null!;

    [ForeignKey(nameof(TimeslotId))]
    [InverseProperty(nameof(Models.Timeslot.SubscriptionEvents))]
    public virtual Timeslot? Timeslot { get; set; }
}
