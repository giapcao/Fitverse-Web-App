using System;
using System.Threading.Tasks;
using Application.Payments.Commands;
using Application.Payments.Models;
using MassTransit;
using MediatR;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Bookings;
using SharedLibrary.Contracts.Payments;

namespace Application.Sagas;

public class PendingSubscriptionPackageSaga : MassTransitStateMachine<PendingSubscriptionPackageSagaData>
{
    public Event<PendingSubscriptionPackageSagaStart> PendingPackageCreated { get; private set; } = null!;

    public PendingSubscriptionPackageSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => PendingPackageCreated,
            config => { config.CorrelateById(context => context.Message.CorrelationId); });

        Initially(
            When(PendingPackageCreated)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.SubscriptionId = context.Message.SubscriptionId;
                    context.Saga.BookingId = context.Message.BookingId;
                    context.Saga.WalletId = context.Message.WalletId;
                    context.Saga.StartedAtUtc = context.Message.StartedAtUtc;
                })
                .ThenAsync(ExecuteSagaTransition)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    private static async Task ExecuteSagaTransition(
        BehaviorContext<PendingSubscriptionPackageSagaData, PendingSubscriptionPackageSagaStart> context)
    {
        if (!context.TryGetPayload(out ConsumeContext<PendingSubscriptionPackageSagaStart>? consumeContext))
        {
            throw new InvalidOperationException("ConsumeContext is required to process the saga message.");
        }
        var serviceProvider = consumeContext.GetPayload<IServiceProvider>();

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var logger = serviceProvider.GetService<ILogger<PendingSubscriptionPackageSaga>>();

        var message = context.Message;

        Error? failure = null;
        bool walletCaptured = false;
        Guid? paymentId = null;
        Guid walletJournalId = Guid.Empty;
        CheckoutDetails? checkout = null;

        try
        {
            var initiateCommand = new InitiatePendingSubscriptionPaymentCommand(
                message.AmountVnd,
                message.Gateway,
                message.BookingId,
                message.Flow,
                message.UserId,
                message.WalletId,
                message.ClientIp);

            var initiationResult = await mediator.Send(initiateCommand, consumeContext.CancellationToken);
            if (initiationResult.IsFailure)
            {
                failure = initiationResult.Error;
            }
            else
            {
                var initiation = initiationResult.Value;
                paymentId = initiation.PaymentId;
                walletJournalId = initiation.WalletJournalId;
                walletCaptured = initiation.BookingWalletCaptured;
                checkout = initiation.Checkout;
                context.Saga.PaymentId = initiation.PaymentId;
                context.Saga.WalletJournalId = initiation.WalletJournalId;
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Pending package saga failed for booking {BookingId}", message.BookingId);
            failure = new Error("Saga.Exception", ex.Message);
        }

        if (failure is not null)
        {
            context.Saga.Status = "Failed";
            context.Saga.FailureCode = failure.Code;
            context.Saga.FailureReason = failure.Description;
            context.Saga.CompletedAtUtc = DateTime.UtcNow;

            await consumeContext.Publish(new PendingSubscriptionPackagePaymentFailed
            {
                CorrelationId = message.CorrelationId,
                SubscriptionId = message.SubscriptionId,
                BookingId = message.BookingId,
                Code = failure.Code,
                Reason = failure.Description,
                FailedAtUtc = context.Saga.CompletedAtUtc.Value
            }, consumeContext.CancellationToken);
        }
        else
        {
            await consumeContext.Publish(new PendingSubscriptionPackagePaymentReady
            {
                CorrelationId = message.CorrelationId,
                SubscriptionId = message.SubscriptionId,
                BookingId = message.BookingId,
                PaymentId = paymentId,
                WalletJournalId = walletJournalId,
                Gateway = message.Gateway,
                CheckoutUrl = checkout?.Url,
                MomoDeeplink = checkout?.Momo?.Deeplink,
                MomoQrCodeUrl = checkout?.Momo?.QrCodeUrl,
                MomoSignature = checkout?.Momo?.Signature,
                PayOsOrderCode = checkout?.PayOs?.OrderCode,
                PayOsPaymentLinkId = checkout?.PayOs?.PaymentLinkId,
                PayOsQrCodeUrl = checkout?.PayOs?.QrCodeUrl,
                WalletCaptured = walletCaptured,
                ReadyAtUtc = DateTime.UtcNow
            }, consumeContext.CancellationToken);

            context.Saga.Status = "Succeeded";
            context.Saga.WalletCaptured = walletCaptured;
            context.Saga.CompletedAtUtc = DateTime.UtcNow;

            await consumeContext.Publish(new PendingSubscriptionPackagePaymentSucceeded
            {
                CorrelationId = message.CorrelationId,
                SubscriptionId = message.SubscriptionId,
                BookingId = message.BookingId,
                PaymentId = paymentId,
                WalletJournalId = walletJournalId,
                WalletCaptured = walletCaptured,
                CompletedAtUtc = context.Saga.CompletedAtUtc.Value
            }, consumeContext.CancellationToken);
        }
    }
}
