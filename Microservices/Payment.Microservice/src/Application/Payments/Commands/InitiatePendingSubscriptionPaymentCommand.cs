using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Messaging;
using Application.Momo;
using Application.Momo.Queries;
using Application.Options;
using Application.PayOs;
using Application.PayOs.Queries;
using Application.Payments.Models;
using Application.Payments.Queries;
using Application.VNPay;
using Application.VNPay.Queries;
using MediatR;
using Microsoft.Extensions.Options;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Commands;

public sealed record InitiatePendingSubscriptionPaymentCommand(
    long AmountVnd,
    Gateway Gateway,
    Guid? BookingId,
    PaymentFlow Flow,
    Guid UserId,
    Guid? WalletId,
    string ClientIp) : ICommand<InitiatePaymentCombinedResponse>;

internal sealed class InitiatePendingSubscriptionPaymentCommandHandler
    : ICommandHandler<InitiatePendingSubscriptionPaymentCommand, InitiatePaymentCombinedResponse>
{
    private readonly IMediator _mediator;
    private readonly IOptions<VNPayOptions> _vnPayOptions;
    private readonly IOptions<MomoOptions> _momoOptions;
    private readonly IOptions<PayOsOptions> _payOsOptions;

    public InitiatePendingSubscriptionPaymentCommandHandler(
        IMediator mediator,
        IOptions<VNPayOptions> vnPayOptions,
        IOptions<MomoOptions> momoOptions,
        IOptions<PayOsOptions> payOsOptions)
    {
        _mediator = mediator;
        _vnPayOptions = vnPayOptions;
        _momoOptions = momoOptions;
        _payOsOptions = payOsOptions;
    }

    public async Task<Result<InitiatePaymentCombinedResponse>> Handle(
        InitiatePendingSubscriptionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Flow != PaymentFlow.BookingByWallet && request.UserId == Guid.Empty)
        {
            return Result.Failure<InitiatePaymentCombinedResponse>(new Error(
                "Payment.UserIdRequired",
                "userId is required for gateway checkout."));
        }

        var initiatePayment = new InitiatePaymentCommand(
            request.AmountVnd,
            request.Gateway,
            request.BookingId,
            request.Flow);

        var initiationResult = await _mediator.Send(initiatePayment, cancellationToken);
        if (initiationResult.IsFailure)
        {
            return Result.Failure<InitiatePaymentCombinedResponse>(initiationResult.Error);
        }

        var initiation = initiationResult.Value;
        CheckoutDetails? checkout = null;
        var bookingWalletCaptured = false;

        if (request.Flow == PaymentFlow.BookingByWallet)
        {
            if (!request.WalletId.HasValue || request.UserId == Guid.Empty)
            {
                return Result.Failure<InitiatePaymentCombinedResponse>(new Error(
                    "Payment.BookingWalletDetailsMissing",
                    "walletId and userId are required for booking-by-wallet payments."));
            }

            var captureCommand = new CaptureBookingWalletPaymentCommand(
                request.WalletId.Value,
                request.UserId,
                initiation.WalletJournalId,
                request.AmountVnd);

            var captureResult = await _mediator.Send(captureCommand, cancellationToken);
            if (captureResult.IsFailure)
            {
                return Result.Failure<InitiatePaymentCombinedResponse>(captureResult.Error);
            }

            bookingWalletCaptured = true;
        }
        else
        {
            if (!initiation.PaymentId.HasValue)
            {
                return Result.Failure<InitiatePaymentCombinedResponse>(new Error(
                    "Payment.PaymentMissing",
                    "Payment identifier is missing from initiation response."));
            }

            var paymentResult = await _mediator.Send(
                new GetPaymentByIdQuery(initiation.PaymentId.Value),
                cancellationToken);

            if (paymentResult.IsFailure)
            {
                return Result.Failure<InitiatePaymentCombinedResponse>(paymentResult.Error);
            }

            var payment = paymentResult.Value;
            switch (payment.Gateway)
            {
                case Gateway.Vnpay:
                    var vnPayOptions = _vnPayOptions.Value;
                    var vnPayConfig = new VnPayConfiguration(
                        vnPayOptions.TmnCode ?? string.Empty,
                        vnPayOptions.HashSecret ?? string.Empty,
                        vnPayOptions.BaseUrl ?? string.Empty,
                        vnPayOptions.ReturnWalletUrl ?? string.Empty,
                        vnPayOptions.ReturnBookingUrl ?? string.Empty,
                        vnPayOptions.IpnUrl ?? string.Empty);

                    if (!vnPayConfig.IsConfiguredFor(request.Flow))
                    {
                        return Result.Failure<InitiatePaymentCombinedResponse>(
                            new Error("Payment.VNPayConfigurationMissing", "VNPay configuration is incomplete."));
                    }

                    var vnPayQuery = new GetVnPayCheckoutUrlQuery(
                        request.AmountVnd,
                        initiation.PaymentId.Value.ToString(),
                        request.ClientIp,
                        request.WalletId,
                        request.UserId,
                        request.Flow,
                        vnPayConfig,
                        DateTime.UtcNow);

                    var vnPayCheckoutResult = await _mediator.Send(vnPayQuery, cancellationToken);
                    if (vnPayCheckoutResult.IsFailure)
                    {
                        return Result.Failure<InitiatePaymentCombinedResponse>(vnPayCheckoutResult.Error);
                    }

                    checkout = new CheckoutDetails(payment.Gateway, vnPayCheckoutResult.Value, null, null);
                    break;

                case Gateway.Momo:
                    var momoOptions = _momoOptions.Value;
                    var momoConfig = new MomoConfiguration(
                        momoOptions.PartnerCode ?? string.Empty,
                        momoOptions.AccessKey ?? string.Empty,
                        momoOptions.SecretKey ?? string.Empty,
                        momoOptions.CreatePaymentUrl ?? string.Empty,
                        momoOptions.RedirectWalletUrl ?? string.Empty,
                        momoOptions.RedirectBookingUrl ?? string.Empty,
                        momoOptions.IpnUrl ?? string.Empty,
                        string.IsNullOrWhiteSpace(momoOptions.RequestType) ? "captureWallet" : momoOptions.RequestType!,
                        string.IsNullOrWhiteSpace(momoOptions.Lang) ? "vi" : momoOptions.Lang!,
                        momoOptions.AutoCapture,
                        string.IsNullOrWhiteSpace(momoOptions.PartnerName) ? null : momoOptions.PartnerName,
                        string.IsNullOrWhiteSpace(momoOptions.StoreId) ? null : momoOptions.StoreId,
                        string.IsNullOrWhiteSpace(momoOptions.ExtraDataTemplate) ? null : momoOptions.ExtraDataTemplate);

                    if (!momoConfig.IsConfiguredFor(request.Flow))
                    {
                        return Result.Failure<InitiatePaymentCombinedResponse>(
                            new Error("Payment.MomoConfigurationMissing", "MoMo configuration is incomplete."));
                    }

                    var orderInfo = BuildMomoOrderInfo(payment, request.Flow);
                    var extraData = BuildMomoExtraData(payment, request.UserId, request.WalletId);

                    var momoQuery = new GetMomoCheckoutUrlQuery(
                        initiation.PaymentId.Value,
                        request.AmountVnd,
                        initiation.PaymentId.Value.ToString(),
                        orderInfo,
                        request.UserId,
                        request.Flow,
                        request.ClientIp,
                        extraData,
                        momoConfig);

                    var momoCheckoutResult = await _mediator.Send(momoQuery, cancellationToken);
                    if (momoCheckoutResult.IsFailure)
                    {
                        return Result.Failure<InitiatePaymentCombinedResponse>(momoCheckoutResult.Error);
                    }

                    checkout = new CheckoutDetails(
                        payment.Gateway,
                        momoCheckoutResult.Value.PayUrl,
                        new MomoCheckoutMeta(
                            momoCheckoutResult.Value.RequestId,
                            momoCheckoutResult.Value.Deeplink,
                            momoCheckoutResult.Value.QrCodeUrl,
                            momoCheckoutResult.Value.Signature),
                        null);
                    break;

                case Gateway.Payos:
                    var payOsOptions = _payOsOptions.Value;
                    var payOsConfig = PayOsConfiguration.FromOptions(payOsOptions);

                    if (!payOsConfig.IsConfiguredFor(request.Flow))
                    {
                        return Result.Failure<InitiatePaymentCombinedResponse>(PayOsErrors.ConfigurationMissing);
                    }

                    var orderCode = PayOsHelper.GenerateOrderCode(payment.Id);
                    var payOsBookingId = payment.BookingId == Guid.Empty ? (Guid?)null : payment.BookingId;
                    var description = PayOsHelper.BuildDescription(payment.Id, orderCode, request.Flow, payOsBookingId);

                    var payOsQuery = new CreatePayOsPaymentLinkQuery(
                        initiation.PaymentId.Value,
                        request.AmountVnd,
                        request.Flow,
                        payOsBookingId,
                        request.UserId,
                        request.WalletId,
                        payOsConfig,
                        orderCode,
                        description);

                    var payOsResult = await _mediator.Send(payOsQuery, cancellationToken);
                    if (payOsResult.IsFailure)
                    {
                        return Result.Failure<InitiatePaymentCombinedResponse>(payOsResult.Error);
                    }

                    checkout = new CheckoutDetails(
                        payment.Gateway,
                        payOsResult.Value.Result.checkoutUrl,
                        null,
                        new PayOsCheckoutMeta(
                            payOsResult.Value.OrderCode,
                            payOsResult.Value.Result.paymentLinkId,
                            payOsResult.Value.Result.qrCode));
                    break;

                default:
                    return Result.Failure<InitiatePaymentCombinedResponse>(new Error(
                        "Payment.GatewayNotSupported",
                        $"Gateway '{payment.Gateway}' is not supported for checkout."));
            }
        }

        return Result.Success(new InitiatePaymentCombinedResponse(
            initiation.PaymentId,
            initiation.WalletJournalId,
            initiation.PaymentStatus,
            initiation.WalletJournalStatus,
            initiation.WalletJournalType,
            checkout,
            bookingWalletCaptured));
    }

    private static string BuildMomoOrderInfo(PaymentResponse payment, PaymentFlow flow)
    {
        var prefix = flow switch
        {
            PaymentFlow.DepositWallet => "Deposit wallet",
            PaymentFlow.PayoutWallet => "Wallet payout",
            PaymentFlow.Booking => "Booking payment",
            PaymentFlow.BookingByWallet => "Wallet booking hold",
            _ => "Payment"
        };

        if (payment.BookingId != Guid.Empty)
        {
            return $"{prefix} - booking {payment.BookingId}";
        }

        return $"{prefix} - payment {payment.Id}";
    }

    private static string BuildMomoExtraData(PaymentResponse payment, Guid userId, Guid? walletId)
    {
        var payload = new Dictionary<string, string>
        {
            ["paymentId"] = payment.Id.ToString(),
            ["userId"] = userId.ToString()
        };

        if (walletId.HasValue)
        {
            payload["walletId"] = walletId.Value.ToString();
        }

        if (payment.BookingId != Guid.Empty)
        {
            payload["bookingId"] = payment.BookingId.ToString();
        }

        var json = JsonSerializer.Serialize(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
}
