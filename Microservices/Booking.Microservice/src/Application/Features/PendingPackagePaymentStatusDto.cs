using System;

namespace Application.Features;

public sealed record PendingPackagePaymentStatusDto(
    Guid BookingId,
    Guid? PaymentId,
    Guid WalletJournalId,
    string Gateway,
    string? CheckoutUrl,
    string? MomoDeeplink,
    string? MomoQrCodeUrl,
    string? MomoSignature,
    long? PayOsOrderCode,
    string? PayOsPaymentLinkId,
    string? PayOsQrCodeUrl,
    bool WalletCaptured,
    DateTime ReadyAtUtc);
