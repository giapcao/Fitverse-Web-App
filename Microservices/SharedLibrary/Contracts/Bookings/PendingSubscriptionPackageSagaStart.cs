using System;
using SharedLibrary.Contracts.Payments;

namespace SharedLibrary.Contracts.Bookings;

public class PendingSubscriptionPackageSagaStart
{
    public Guid CorrelationId { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid CoachId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? RequestedBookingId { get; set; }
    public Guid? WalletId { get; set; }
    public Guid? TimeslotId { get; set; }
    public long AmountVnd { get; set; }
    public Gateway Gateway { get; set; }
    public PaymentFlow Flow { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public string ClientIp { get; set; } = "127.0.0.1";
}
