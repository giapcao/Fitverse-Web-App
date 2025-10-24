namespace WebApi.Options;

public sealed class MomoOptions
{
    public const string SectionName = "Momo";

    public string PartnerCode { get; set; } = string.Empty;

    public string PartnerName { get; set; } = string.Empty;

    public string StoreId { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string CreatePaymentUrl { get; set; } = string.Empty;

    public string RedirectWalletUrl { get; set; } = string.Empty;

    public string RedirectBookingUrl { get; set; } = string.Empty;

    public string IpnUrl { get; set; } = string.Empty;

    public string RequestType { get; set; } = "captureWallet";

    public string Lang { get; set; } = "vi";

    public bool AutoCapture { get; set; } = true;

    public string ExtraDataTemplate { get; set; } = string.Empty;
}
