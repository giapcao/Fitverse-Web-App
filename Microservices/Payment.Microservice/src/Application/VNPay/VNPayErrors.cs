using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.VNPay;

public static class VnPayErrors
{
    public static readonly Error ConfigurationMissing =
        new("VNPay.ConfigurationMissing", "VNPay configuration is incomplete.");

    public static readonly Error AmountMustBeGreaterThanZero =
        new("VNPay.InvalidAmount", "amountVnd must be greater than zero.");

    public static readonly Error AmountTooLarge =
        new("VNPay.AmountTooLarge", "amountVnd is too large.");

    public static readonly Error OrderIdRequired =
        new("VNPay.OrderIdRequired", "orderId is required.");

}
