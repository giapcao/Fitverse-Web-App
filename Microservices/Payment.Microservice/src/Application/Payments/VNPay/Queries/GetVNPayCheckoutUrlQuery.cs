using System;
using System.Net;
using System.Net.Sockets;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.VNPay.Queries;

public sealed record GetVnPayCheckoutUrlQuery(
    long AmountVnd,
    string OrderId,
    string ClientIp,
    VNPayConfiguration Configuration,
    DateTime RequestedAtUtc) : IQuery<string>;

internal sealed class GetVnPayCheckoutUrlQueryHandler : IQueryHandler<GetVnPayCheckoutUrlQuery, string>
{
    private readonly ILogger<GetVnPayCheckoutUrlQueryHandler> _logger;

    public GetVnPayCheckoutUrlQueryHandler(ILogger<GetVnPayCheckoutUrlQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<string>> Handle(GetVnPayCheckoutUrlQuery request, CancellationToken cancellationToken)
    {
        var configuration = request.Configuration;
        if (!configuration.IsComplete)
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.ConfigurationMissing));
        }

        if (request.AmountVnd <= 0)
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.AmountMustBeGreaterThanZero));
        }

        var orderId = request.OrderId?.Trim();
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return Task.FromResult(Result.Failure<string>(VnPayErrors.OrderIdRequired));
        }

        try
        {
            var signedRequest = VnPayHelper.BuildPaymentUrl(
                configuration,
                request.AmountVnd,
                orderId,
                NormalizeClientIp(request.ClientIp),
                request.RequestedAtUtc);

            _logger.LogInformation(
                "VNPay checkout rawQuery={RawQuery} secureHash={SecureHash}",
                signedRequest.RawQuery,
                signedRequest.SecureHash);

            return Task.FromResult(Result.Success(signedRequest.PaymentUrl));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "VNPay checkout URL generation failed due to invalid configuration.");
            return Task.FromResult(Result.Failure<string>(VnPayErrors.ConfigurationMissing));
        }
    }

    private static string NormalizeClientIp(string? clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp))
        {
            return "127.0.0.1";
        }

        clientIp = clientIp.Trim();
        if (IPAddress.TryParse(clientIp, out var address))
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return address.ToString();
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return "127.0.0.1";
            }
        }

        var segments = clientIp.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            if (IPAddress.TryParse(segment, out var ipv4) && ipv4.AddressFamily == AddressFamily.InterNetwork)
            {
                return ipv4.ToString();
            }
        }

        return "127.0.0.1";
    }
}
