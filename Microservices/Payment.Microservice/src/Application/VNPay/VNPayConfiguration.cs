using Application.Payments;
using Application.Payments.Common;

namespace Application.VNPay;

public sealed record VnPayConfiguration(
    string TmnCode,
    string HashSecret,
    string BaseUrl,
    string ReturnWalletUrl,
    string ReturnBookingUrl,
    string IpnUrl) : IPaymentGatewayConfiguration
{
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(TmnCode) &&
        !string.IsNullOrWhiteSpace(HashSecret) &&
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(IpnUrl) &&
        (!string.IsNullOrWhiteSpace(ReturnWalletUrl) || !string.IsNullOrWhiteSpace(ReturnBookingUrl));

    public bool IsConfiguredFor(PaymentFlow flow)
    {
        var returnUrl = GetReturnUrl(flow);

        return !string.IsNullOrWhiteSpace(TmnCode) &&
               !string.IsNullOrWhiteSpace(HashSecret) &&
               !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(IpnUrl) &&
               !string.IsNullOrWhiteSpace(returnUrl);
    }

    public string? GetReturnUrl(PaymentFlow flow) =>
        flow switch
        {
            PaymentFlow.DepositWallet => ReturnWalletUrl,
            PaymentFlow.PayoutWallet => ReturnWalletUrl,
            PaymentFlow.Booking => ReturnBookingUrl,
            _ => null
        };
}
