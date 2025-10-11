using System;
using System.Collections.Generic;
using System.Globalization;
using Application.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.VNPay.Queries;

public sealed record GetVNPayCheckoutUrlQuery(
    long AmountVnd,
    string OrderId,
    string ClientIp,
    VNPayConfiguration Configuration,
    DateTime RequestedAtUtc) : IQuery<string>;

internal sealed class GetVNPayCheckoutUrlQueryHandler : IQueryHandler<GetVNPayCheckoutUrlQuery, string>
{
    public Task<Result<string>> Handle(GetVNPayCheckoutUrlQuery request, CancellationToken cancellationToken)
    {
        if (!request.Configuration.IsComplete)
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.ConfigurationMissing));
        }

        if (request.AmountVnd <= 0)
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.AmountMustBeGreaterThanZero));
        }

        if (string.IsNullOrWhiteSpace(request.OrderId))
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.OrderIdRequired));
        }

        long amountInMinorUnits;
        try
        {
            amountInMinorUnits = checked(request.AmountVnd * 100);
        }
        catch (OverflowException)
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.AmountTooLarge));
        }

        var vietnamTimeZone = VnPayHelper.GetVietnamTimeZone();
        var nowVietnam = TimeZoneInfo.ConvertTime(request.RequestedAtUtc, vietnamTimeZone);
        var createDate = nowVietnam.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var expireDate = nowVietnam.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = request.Configuration.TmnCode,
            ["vnp_Amount"] = amountInMinorUnits.ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = request.OrderId,
            ["vnp_OrderInfo"] = $"Payment for order {request.OrderId}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = request.Configuration.ReturnUrl,
            ["vnp_IpnUrl"] = request.Configuration.IpnUrl,
            ["vnp_CreateDate"] = createDate,
            ["vnp_ExpireDate"] = expireDate,
            ["vnp_IpAddr"] = request.ClientIp
        };

        var paymentUrl = VnPayHelper.CreatePaymentUrl(
            request.Configuration.BaseUrl,
            request.Configuration.HashSecret,
            parameters);

        return Task.FromResult(Result.Success(paymentUrl));
    }
}
