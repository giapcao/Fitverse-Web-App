using Application.Payments.Common;
using Domain.Enums;

namespace Application.Payments.Returns;

public sealed record PaymentGatewayReturnContext(
    Gateway Gateway,
    IReadOnlyDictionary<string, string> QueryParameters,
    IPaymentGatewayConfiguration Configuration,
    Guid? UserId);

public sealed record PaymentGatewayReturnResult(bool TransactionCaptured, Guid? UserId);
