using Application.Options;
using Application.Payments.Common;
using Net.payOS;
using SharedLibrary.Contracts.Payments;

namespace Application.PayOs;

public sealed record PayOsConfiguration(
    string ClientId,
    string ApiKey,
    string ChecksumKey,
    string? ReturnWalletUrl,
    string? CancelWalletUrl,
    string? ReturnBookingUrl,
    string? CancelBookingUrl) : IPaymentGatewayConfiguration
{
    public bool IsConfiguredFor(PaymentFlow flow)
    {
        return flow switch
        {
            PaymentFlow.DepositWallet => HasWalletRoutes,
            PaymentFlow.PayoutWallet => false,
            PaymentFlow.Booking => HasBookingRoutes,
            PaymentFlow.BookingByWallet => false,
            _ => false
        } && HasCredentials;
    }

    public bool HasCredentials =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ChecksumKey);

    private bool HasWalletRoutes =>
        !string.IsNullOrWhiteSpace(ReturnWalletUrl) &&
        !string.IsNullOrWhiteSpace(CancelWalletUrl);

    private bool HasBookingRoutes =>
        !string.IsNullOrWhiteSpace(ReturnBookingUrl) &&
        !string.IsNullOrWhiteSpace(CancelBookingUrl);

    public PayOS CreateClient()
    {
        return new PayOS(ClientId, ApiKey, ChecksumKey);
    }

    public string? GetReturnUrl(
        PaymentFlow flow,
        Guid paymentId,
        long orderCode,
        Guid? bookingId,
        Guid? walletId,
        Guid userId)
    {
        var template = flow switch
        {
            PaymentFlow.DepositWallet => ReturnWalletUrl,
            PaymentFlow.Booking => ReturnBookingUrl,
            _ => null
        };

        return PayOsHelper.ApplyTemplate(
            template,
            paymentId,
            orderCode,
            bookingId,
            walletId,
            userId);
    }

    public string? GetCancelUrl(
        PaymentFlow flow,
        Guid paymentId,
        long orderCode,
        Guid? bookingId,
        Guid? walletId,
        Guid userId)
    {
        var template = flow switch
        {
            PaymentFlow.DepositWallet => CancelWalletUrl,
            PaymentFlow.Booking => CancelBookingUrl,
            _ => null
        };

        return PayOsHelper.ApplyTemplate(
            template,
            paymentId,
            orderCode,
            bookingId,
            walletId,
            userId);
    }

    public static PayOsConfiguration FromOptions(PayOsOptions options)
    {
        return new PayOsConfiguration(
            options.ClientId ?? string.Empty,
            options.ApiKey ?? string.Empty,
            options.ChecksumKey ?? string.Empty,
            options.ReturnWalletUrl,
            options.CancelWalletUrl,
            options.ReturnBookingUrl,
            options.CancelBookingUrl);
    }
}
