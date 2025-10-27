
using Domain.Enums;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Models;

public sealed record InitiatePaymentRequest(
    long AmountVnd,
    Gateway Gateway,
    Guid? BookingId,
    PaymentFlow Flow,
    Guid UserId,
    Guid? WalletId);

public sealed record InitiatePaymentCombinedResponse(
    Guid? PaymentId,
    Guid WalletJournalId,
    PaymentStatus PaymentStatus,
    WalletJournalStatus WalletJournalStatus,
    WalletJournalType WalletJournalType,
    CheckoutDetails? Checkout,
    bool BookingWalletCaptured);

public sealed record CheckoutDetails(
    Gateway Gateway,
    string Url,
    MomoCheckoutMeta? Momo,
    PayOsCheckoutMeta? PayOs);

public sealed record MomoCheckoutMeta(
    string? RequestId,
    string? Deeplink,
    string? QrCodeUrl,
    string? Signature);

public sealed record PayOsCheckoutMeta(
    long OrderCode,
    string? PaymentLinkId,
    string? QrCodeUrl);
