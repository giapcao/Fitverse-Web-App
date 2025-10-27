using SharedLibrary.Common.ResponseModel;

namespace Application.PayOs;

public static class PayOsErrors
{
    public static readonly Error ConfigurationMissing = new(
        "Payment.PayOsConfigurationMissing",
        "PayOS configuration is incomplete.");

    public static readonly Error AmountMustBeGreaterThanZero = new(
        "Payment.PayOsAmountMustBeGreaterThanZero",
        "Amount must be greater than zero for PayOS checkout.");

    public static readonly Error AmountExceedsLimit = new(
        "Payment.PayOsAmountExceedsLimit",
        "Amount exceeds PayOS maximum supported value.");

    public static Error RequestFailed(string message) =>
        new("Payment.PayOsRequestFailed", $"PayOS request failed: {message}");
}
