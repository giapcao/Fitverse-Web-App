using System;

namespace SharedLibrary.Contracts.Bookings;

public class PendingSubscriptionPackagePaymentFailed
{
    public Guid CorrelationId { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid BookingId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAtUtc { get; set; }
}

