using Application.Payments.Common;
using SharedLibrary.Contracts.Payments;

namespace Application.Momo;

public sealed record MomoConfiguration(
    string PartnerCode,
    string AccessKey,
    string SecretKey,
    string CreatePaymentUrl,
    string RedirectWalletUrl,
    string RedirectBookingUrl,
    string IpnUrl,
    string RequestType,
    string Lang,
    bool AutoCapture,
    string? PartnerName,
    string? StoreId,
    string? ExtraDataTemplate) : IPaymentGatewayConfiguration
{
    public bool IsConfiguredFor(PaymentFlow flow)
    {
        var redirectUrl = GetRedirectUrl(flow);

        return !string.IsNullOrWhiteSpace(PartnerCode) &&
               !string.IsNullOrWhiteSpace(AccessKey) &&
               !string.IsNullOrWhiteSpace(SecretKey) &&
               !string.IsNullOrWhiteSpace(CreatePaymentUrl) &&
               !string.IsNullOrWhiteSpace(IpnUrl) &&
               !string.IsNullOrWhiteSpace(RequestType) &&
               !string.IsNullOrWhiteSpace(Lang) &&
               !string.IsNullOrWhiteSpace(redirectUrl);
    }

    public string? GetRedirectUrl(PaymentFlow flow) =>
        flow switch
        {
            PaymentFlow.DepositWallet => RedirectWalletUrl,
            PaymentFlow.PayoutWallet => RedirectWalletUrl,
            PaymentFlow.Booking => RedirectBookingUrl,
            PaymentFlow.BookingByWallet => RedirectBookingUrl,
            _ => RedirectWalletUrl
        };
}
