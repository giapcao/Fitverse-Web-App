using System.Threading.Tasks;
using Application.Bookings.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts.Bookings;

namespace Application.Consumers;

public class PendingSubscriptionPackagePaymentFailedConsumer
    : IConsumer<PendingSubscriptionPackagePaymentFailed>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PendingSubscriptionPackagePaymentFailedConsumer> _logger;

    public PendingSubscriptionPackagePaymentFailedConsumer(
        IMediator mediator,
        ILogger<PendingSubscriptionPackagePaymentFailedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PendingSubscriptionPackagePaymentFailed> context)
    {
        var message = context.Message;
        var command = new CancelPendingSubscriptionBookingCommand(
            message.SubscriptionId,
            message.BookingId);

        var result = await _mediator.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError(
                "Saga cancel pending package failed for booking {BookingId} - {Code}: {Description}",
                message.BookingId,
                result.Error.Code,
                result.Error.Description);
        }
    }
}
