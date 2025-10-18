using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Returns;

public interface IPaymentGatewayReturnHandler
{
    Domain.Enums.Gateway Gateway { get; }

    Task<Result<PaymentGatewayReturnResult>> HandleAsync(
        PaymentGatewayReturnContext context,
        CancellationToken cancellationToken);
}
