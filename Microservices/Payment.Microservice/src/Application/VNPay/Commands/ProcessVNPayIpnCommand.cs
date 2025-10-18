using System.Globalization;
using Application.Abstractions.Messaging;
using Application.Payments.VNPay;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.VNPay.Commands;

public sealed record VnPayIpnResponse(string RspCode, string Message);

public sealed record ProcessVnPayIpnCommand(
    IReadOnlyDictionary<string, string> QueryParameters,
    VNPayConfiguration Configuration) : ICommand<VnPayIpnResponse>;

internal sealed class ProcessVnPayIpnCommandHandler : ICommandHandler<ProcessVnPayIpnCommand, VnPayIpnResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessVnPayIpnCommandHandler> _logger;

    public ProcessVnPayIpnCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessVnPayIpnCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<VnPayIpnResponse>> Handle(ProcessVnPayIpnCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Configuration.HashSecret) ||
            string.IsNullOrWhiteSpace(request.Configuration.TmnCode))
        {
            return Result.Success(new VnPayIpnResponse("97", "Invalid signature"));
        }

        var query = new Dictionary<string, string>(request.QueryParameters, StringComparer.OrdinalIgnoreCase);
        if (!VnPayHelper.ValidateSignature(query, request.Configuration.HashSecret))
        {
            _logger.LogWarning("VNPay IPN signature invalid for payload {@Payload}", query);
            return Result.Success(new VnPayIpnResponse("97", "Invalid signature"));
        }

        if (!TryResolvePaymentId(query, out var paymentId))
        {
            return Result.Success(new VnPayIpnResponse("01", "Order not found"));
        }

        var payments = await _paymentRepository.FindAsync(payment => payment.Id == paymentId, cancellationToken);
        var payment = payments.FirstOrDefault();
        if (payment is null)
        {
            return Result.Success(new VnPayIpnResponse("01", "Order not found"));
        }

        if (!TryNormalizeAmount(query, out var normalizedAmount))
        {
            return Result.Success(new VnPayIpnResponse("04", "Invalid amount"));
        }

        if (payment.AmountVnd != normalizedAmount)
        {
            return Result.Success(new VnPayIpnResponse("04", "Invalid amount"));
        }

        if (payment.Status == PaymentStatus.Captured)
        {
            return Result.Success(new VnPayIpnResponse("02", "Order already confirmed"));
        }

        var responseCode = query.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        var transactionStatus = query.TryGetValue("vnp_TransactionStatus", out var status) ? status : string.Empty;
        if (!VnPayHelper.IsSuccess(responseCode, transactionStatus))
        {
            return Result.Success(new VnPayIpnResponse("00", "Payment status acknowledged"));
        }

        payment.Status = PaymentStatus.Captured;
        payment.Gateway = Gateway.Vnpay;

        if (query.TryGetValue("vnp_TransactionNo", out var transactionNo) && !string.IsNullOrWhiteSpace(transactionNo))
        {
            payment.GatewayTxnId = transactionNo;
        }

        payment.PaidAt = DateTime.UtcNow;

        _paymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new VnPayIpnResponse("00", "Success"));
    }

    private static bool TryNormalizeAmount(IReadOnlyDictionary<string, string> query, out long amountVnd)
    {
        if (query.TryGetValue("vnp_Amount", out var rawAmount) &&
            long.TryParse(rawAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amountFromGateway) &&
            amountFromGateway % 100 == 0)
        {
            amountVnd = amountFromGateway / 100;
            return true;
        }

        amountVnd = 0;
        return false;
    }

    private static bool TryResolvePaymentId(IReadOnlyDictionary<string, string> query, out Guid paymentId)
    {
        if (query.TryGetValue("vnp_TxnRef", out var txnRef) && Guid.TryParse(txnRef, out paymentId))
        {
            return true;
        }

        if (query.TryGetValue("vnp_OrderInfo", out var orderInfo))
        {
            var trailing = ExtractTrailingToken(orderInfo);
            if (Guid.TryParse(trailing, out paymentId))
            {
                return true;
            }
        }

        paymentId = Guid.Empty;
        return false;
    }

    private static string? ExtractTrailingToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var token = value
            .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        return token;
    }
}
