namespace WebApi.Options;

public sealed class VNPayOptions
{
    public const string SectionName = "VNPay";

    public string TmnCode { get; set; } = string.Empty;

    public string HashSecret { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string ReturnWalletUrl { get; set; } = string.Empty;

    public string ReturnBookingUrl { get; set; } = string.Empty;

    public string IpnUrl { get; set; } = string.Empty;
}
