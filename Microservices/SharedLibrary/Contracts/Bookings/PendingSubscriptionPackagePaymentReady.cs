using System;
using SharedLibrary.Contracts.Payments;

namespace SharedLibrary.Contracts.Bookings;

public class PendingSubscriptionPackagePaymentReady
{
    public Guid CorrelationId { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid WalletJournalId { get; set; }
    public Gateway Gateway { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? MomoDeeplink { get; set; }
    public string? MomoQrCodeUrl { get; set; }
    public string? MomoSignature { get; set; }
    public bool WalletCaptured { get; set; }
    public DateTime ReadyAtUtc { get; set; }
}

