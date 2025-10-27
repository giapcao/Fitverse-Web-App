using Application.Abstractions.Messaging;
using Application.Payments.Common;
using Domain.Enums;
using SharedLibrary.Common.ResponseModel;
using System.Linq;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Returns;

public sealed record ProcessPaymentGatewayReturnCommand(
    Gateway Gateway,
    IReadOnlyDictionary<string, string> QueryParameters,
    IPaymentGatewayConfiguration Configuration,
    Guid? UserId) : ICommand<PaymentGatewayReturnResult>;

internal sealed class ProcessPaymentGatewayReturnCommandHandler
    : ICommandHandler<ProcessPaymentGatewayReturnCommand, PaymentGatewayReturnResult>
{
    private readonly IReadOnlyDictionary<Gateway, IPaymentGatewayReturnHandler> _handlers;

    public ProcessPaymentGatewayReturnCommandHandler(IEnumerable<IPaymentGatewayReturnHandler> handlers)
    {
        _handlers = handlers.ToDictionary(handler => handler.Gateway, handler => handler, GatewayComparer.Instance);
    }

    public Task<Result<PaymentGatewayReturnResult>> Handle(ProcessPaymentGatewayReturnCommand request, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(request.Gateway, out var handler))
        {
            return Task.FromResult(Result.Failure<PaymentGatewayReturnResult>(PaymentReturnErrors.HandlerNotFound(request.Gateway)));
        }

        var context = new PaymentGatewayReturnContext(
            request.Gateway,
            request.QueryParameters,
            request.Configuration,
            request.UserId);

        return handler.HandleAsync(context, cancellationToken);
    }

    private sealed class GatewayComparer : IEqualityComparer<Gateway>
    {
        public static readonly GatewayComparer Instance = new();

        public bool Equals(Gateway x, Gateway y) => x == y;

        public int GetHashCode(Gateway obj) => (int)obj;
    }
}
