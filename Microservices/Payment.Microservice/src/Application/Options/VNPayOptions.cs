namespace Application.Options;

public sealed class VNPayOptions
{
    public const string SectionName = "VNPay";

    public string? TmnCode { get; set; }
    public string? HashSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string? ReturnWalletUrl { get; set; }
    public string? ReturnBookingUrl { get; set; }
    public string? IpnUrl { get; set; }
}
