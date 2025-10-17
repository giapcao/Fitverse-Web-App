using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Persistence.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence.Models;

[Table("subscription")]
[Index(nameof(CoachId), nameof(PeriodStart), Name = "idx_subscription_coach")]
[Index(nameof(UserId), nameof(PeriodStart), Name = "idx_subscription_user")]
[Index(nameof(UserId), nameof(CoachId), nameof(ServiceId), nameof(PeriodStart), nameof(PeriodEnd), IsUnique = true, Name = "ux_subscription_unique_period")]
public class Subscription
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

    [Column("status")]
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    [Column("period_start")]
    public DateTime PeriodStart { get; set; }

    [Column("period_end")]
    public DateTime PeriodEnd { get; set; }

    [Column("sessions_total")]
    public int SessionsTotal { get; set; }

    [Column("sessions_reserved")]
    public int SessionsReserved { get; set; } = 0;

    [Column("sessions_consumed")]
    public int SessionsConsumed { get; set; } = 0;

    [Column("price_gross_vnd")]
    public long PriceGrossVnd { get; set; }

    [Column("commission_pct")]
    [Precision(5, 2)]
    public decimal CommissionPct { get; set; } = 15.00m;

    [Column("commission_vnd")]
    public long CommissionVnd { get; set; }

    [Column("net_amount_vnd")]
    public long NetAmountVnd { get; set; }

    [Column("currency_code")]
    public string CurrencyCode { get; set; } = "VND";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [InverseProperty(nameof(SubscriptionEvent.Subscription))]
    public virtual ICollection<SubscriptionEvent> SubscriptionEvents { get; set; } = new HashSet<SubscriptionEvent>();
}
