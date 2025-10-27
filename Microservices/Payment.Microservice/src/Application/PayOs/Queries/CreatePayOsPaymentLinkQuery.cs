using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Net.payOS.Errors;
using Net.payOS;
using Net.payOS.Types;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.PayOs.Queries;

public sealed record CreatePayOsPaymentLinkQuery(
    Guid PaymentId,
    long AmountVnd,
    PaymentFlow Flow,
    Guid? BookingId,
    Guid UserId,
    Guid? WalletId,
    PayOsConfiguration Configuration,
    long OrderCode,
    string Description) : IQuery<PayOsCheckoutResponse>;

public sealed record PayOsCheckoutResponse(
    CreatePaymentResult Result,
    long OrderCode);

internal sealed class CreatePayOsPaymentLinkQueryHandler
    : IQueryHandler<CreatePayOsPaymentLinkQuery, PayOsCheckoutResponse>
{
    public async Task<Result<PayOsCheckoutResponse>> Handle(
        CreatePayOsPaymentLinkQuery request,
        CancellationToken cancellationToken)
    {
        if (request.AmountVnd <= 0)
        {
            return Result.Failure<PayOsCheckoutResponse>(PayOsErrors.AmountMustBeGreaterThanZero);
        }

        if (request.AmountVnd > int.MaxValue)
        {
            return Result.Failure<PayOsCheckoutResponse>(PayOsErrors.AmountExceedsLimit);
        }

        var configuration = request.Configuration;
        if (!configuration.IsConfiguredFor(request.Flow))
        {
            return Result.Failure<PayOsCheckoutResponse>(PayOsErrors.ConfigurationMissing);
        }

        var returnUrl = configuration.GetReturnUrl(
            request.Flow,
            request.PaymentId,
            request.OrderCode,
            request.BookingId,
            request.WalletId,
            request.UserId);

        var cancelUrl = configuration.GetCancelUrl(
            request.Flow,
            request.PaymentId,
            request.OrderCode,
            request.BookingId,
            request.WalletId,
            request.UserId);

        if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(cancelUrl))
        {
            return Result.Failure<PayOsCheckoutResponse>(PayOsErrors.ConfigurationMissing);
        }

        var client = configuration.CreateClient();
        var items = new List<ItemData>
        {
            new(request.Description, 1, (int)request.AmountVnd)
        };

        var paymentData = new PaymentData(
            request.OrderCode,
            (int)request.AmountVnd,
            request.Description,
            items,
            cancelUrl,
            returnUrl);

        try
        {
            var response = await client.createPaymentLink(paymentData);
            return Result.Success(new PayOsCheckoutResponse(response, request.OrderCode));
        }
        catch (PayOSError error)
        {
            return Result.Failure<PayOsCheckoutResponse>(PayOsErrors.RequestFailed(error.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<PayOsCheckoutResponse>(PayOsErrors.RequestFailed(ex.Message));
        }
    }
}
