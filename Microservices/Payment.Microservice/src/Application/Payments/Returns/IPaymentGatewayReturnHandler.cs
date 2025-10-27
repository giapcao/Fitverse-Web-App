using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Returns;

public interface IPaymentGatewayReturnHandler
{
    Gateway Gateway { get; }

    Task<Result<PaymentGatewayReturnResult>> HandleAsync(
        PaymentGatewayReturnContext context,
        CancellationToken cancellationToken);
}
