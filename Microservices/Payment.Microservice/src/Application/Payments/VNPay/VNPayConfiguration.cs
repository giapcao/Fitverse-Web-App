namespace Application.Payments.VNPay;

public sealed record VNPayConfiguration(
    string TmnCode,
    string HashSecret,
    string BaseUrl,
    string ReturnUrl,
    string IpnUrl)
{
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(TmnCode) &&
        !string.IsNullOrWhiteSpace(HashSecret) &&
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(ReturnUrl) &&
        !string.IsNullOrWhiteSpace(IpnUrl);
}
