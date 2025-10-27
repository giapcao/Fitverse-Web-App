using SharedLibrary.Common.ResponseModel;

namespace Application.Momo;

public static class MomoErrors
{
    public static readonly Error ConfigurationMissing =
        new("Momo.ConfigurationMissing", "MoMo configuration is missing or incomplete.");

    public static readonly Error AmountMustBeGreaterThanZero =
        new("Momo.AmountMustBeGreaterThanZero", "Amount must be greater than zero.");

    public static readonly Error OrderIdRequired =
        new("Momo.OrderIdRequired", "OrderId is required.");

    public static Error RequestFailed(int resultCode, string? message) =>
        new("Momo.RequestFailed",
            $"MoMo request failed with resultCode={resultCode}. {message}".Trim());

    public static readonly Error UnexpectedResponse =
        new("Momo.UnexpectedResponse", "Unable to read MoMo response.");
}
