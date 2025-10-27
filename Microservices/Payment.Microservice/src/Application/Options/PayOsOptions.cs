namespace Application.Options;

public sealed class PayOsOptions
{
    public const string SectionName = "PayOs";

    public string? ClientId { get; set; }
    public string? ApiKey { get; set; }
    public string? ChecksumKey { get; set; }

    public string? ReturnWalletUrl { get; set; }
    public string? CancelWalletUrl { get; set; }
    public string? ReturnBookingUrl { get; set; }
    public string? CancelBookingUrl { get; set; }
}
