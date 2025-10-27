using System.Threading.Tasks;
using Application.Bookings.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts.Bookings;

namespace Application.Consumers;

public class PendingSubscriptionPackagePaymentSucceededConsumer
    : IConsumer<PendingSubscriptionPackagePaymentSucceeded>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PendingSubscriptionPackagePaymentSucceededConsumer> _logger;

    public PendingSubscriptionPackagePaymentSucceededConsumer(
        IMediator mediator,
        ILogger<PendingSubscriptionPackagePaymentSucceededConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PendingSubscriptionPackagePaymentSucceeded> context)
    {
        var message = context.Message;
        var command = new ConfirmPendingSubscriptionBookingCommand(
            message.SubscriptionId,
            message.BookingId);

        var result = await _mediator.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError(
                "Saga confirm pending package failed for booking {BookingId} - {Code}: {Description}",
                message.BookingId,
                result.Error.Code,
                result.Error.Description);
        }
    }
}

