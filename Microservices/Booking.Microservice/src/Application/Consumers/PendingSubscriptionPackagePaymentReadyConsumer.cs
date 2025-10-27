using System.Threading.Tasks;
using Application.Common.Stores;
using Application.Features;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts.Bookings;

namespace Application.Consumers;

public sealed class PendingSubscriptionPackagePaymentReadyConsumer
    : IConsumer<PendingSubscriptionPackagePaymentReady>
{
    private readonly IPendingPackagePaymentStatusStore _store;
    private readonly ILogger<PendingSubscriptionPackagePaymentReadyConsumer> _logger;

    public PendingSubscriptionPackagePaymentReadyConsumer(
        IPendingPackagePaymentStatusStore store,
        ILogger<PendingSubscriptionPackagePaymentReadyConsumer> logger)
    {
        _store = store;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<PendingSubscriptionPackagePaymentReady> context)
    {
        var message = context.Message;
        var dto = new PendingPackagePaymentStatusDto(
            message.BookingId,
            message.PaymentId,
            message.WalletJournalId,
            message.Gateway.ToString(),
            message.CheckoutUrl,
            message.MomoDeeplink,
            message.MomoQrCodeUrl,
            message.MomoSignature,
            message.PayOsOrderCode,
            message.PayOsPaymentLinkId,
            message.PayOsQrCodeUrl,
            message.WalletCaptured,
            message.ReadyAtUtc);

        _store.Set(message.BookingId, dto);
        _logger.LogInformation(
            "Pending subscription payment ready for booking {BookingId} (paymentId: {PaymentId})",
            message.BookingId,
            message.PaymentId);

        return Task.CompletedTask;
    }
}
