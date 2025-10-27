namespace Application.Options;

public sealed class MomoOptions
{
    public const string SectionName = "Momo";

    public string? PartnerCode { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? CreatePaymentUrl { get; set; }
    public string? RedirectWalletUrl { get; set; }
    public string? RedirectBookingUrl { get; set; }
    public string? IpnUrl { get; set; }
    public string? RequestType { get; set; }
    public string? Lang { get; set; }
    public bool AutoCapture { get; set; }
    public string? PartnerName { get; set; }
    public string? StoreId { get; set; }
    public string? ExtraDataTemplate { get; set; }
}
