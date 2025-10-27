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
    bool WalletCaptured,
    DateTime ReadyAtUtc);
