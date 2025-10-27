using System;

namespace SharedLibrary.Contracts.Bookings;

public class PendingSubscriptionPackagePaymentSucceeded
{
    public Guid CorrelationId { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid WalletJournalId { get; set; }
    public bool WalletCaptured { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}

