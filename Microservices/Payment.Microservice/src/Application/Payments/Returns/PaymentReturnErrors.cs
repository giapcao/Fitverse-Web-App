using Domain.Enums;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Returns;

public static class PaymentReturnErrors
{
    public static Error HandlerNotFound(Gateway gateway) =>
        new("PaymentReturn.HandlerNotFound", $"No payment gateway return handler registered for gateway '{gateway}'.");

    public static Error InvalidConfiguration(Gateway gateway) =>
        new("PaymentReturn.InvalidConfiguration", $"The configuration provided for gateway '{gateway}' is invalid.");
}
