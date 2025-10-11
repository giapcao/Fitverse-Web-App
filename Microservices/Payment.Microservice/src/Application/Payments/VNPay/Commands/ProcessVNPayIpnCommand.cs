using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.VNPay.Commands;

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
        if (string.IsNullOrWhiteSpace(request.Configuration.HashSecret))
        {
            return Result.Success(new VnPayIpnResponse("97", "Invalid signature"));
        }

        var query = new Dictionary<string, string>(request.QueryParameters, StringComparer.Ordinal);
        if (!query.TryGetValue("vnp_SecureHash", out var secureHash))
        {
            return Result.Success(new VnPayIpnResponse("97", "Invalid signature"));
        }

        var filtered = query
            .Where(kvp => !string.Equals(kvp.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

        var dataToVerify = VnPayHelper.BuildDataToVerify(filtered);
        var calculatedHash = VnPayHelper.HmacSha512(request.Configuration.HashSecret, dataToVerify);
        _logger.LogInformation(
            "VNPay IPN signature verification - data: {Data}, calculated hash: {CalculatedHash}, provided hash: {ProvidedHash}",
            dataToVerify,
            calculatedHash,
            secureHash);

        var isSignatureValid = string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
        if (!isSignatureValid)
        {
            return Result.Success(new VnPayIpnResponse("97", "Invalid signature"));
        }

        if (!query.TryGetValue("vnp_TxnRef", out var txnRef) || string.IsNullOrWhiteSpace(txnRef))
        {
            return Result.Success(new VnPayIpnResponse("01", "Order not found"));
        }

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return Result.Success(new VnPayIpnResponse("01", "Order not found"));
        }

        var payments = await _paymentRepository.FindAsync(payment => payment.Id == paymentId, cancellationToken);
        var payment = payments.FirstOrDefault();
        if (payment is null)
        {
            return Result.Success(new VnPayIpnResponse("01", "Order not found"));
        }

        if (!query.TryGetValue("vnp_Amount", out var amountRaw) ||
            !long.TryParse(amountRaw, out var amountFromGateway))
        {
            return Result.Success(new VnPayIpnResponse("04", "Invalid amount"));
        }

        if (amountFromGateway % 100 != 0)
        {
            return Result.Success(new VnPayIpnResponse("04", "Invalid amount"));
        }

        var normalizedAmount = amountFromGateway / 100;
        if (payment.AmountVnd != normalizedAmount)
        {
            return Result.Success(new VnPayIpnResponse("04", "Invalid amount"));
        }

        if (payment.Status == PaymentStatus.Captured)
        {
            return Result.Success(new VnPayIpnResponse("02", "Order already confirmed"));
        }

        var responseCode = query.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        if (!string.Equals(responseCode, "00", StringComparison.Ordinal))
        {
            return Result.Success(new VnPayIpnResponse("00", "Payment status acknowledged"));
        }

        payment.Status = PaymentStatus.Captured;
        payment.Gateway = Gateway.Vnpay;
        payment.GatewayTxnId = query.TryGetValue("vnp_TransactionNo", out var txnNo) ? txnNo : payment.GatewayTxnId;
        payment.PaidAt = DateTime.UtcNow;

        _paymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new VnPayIpnResponse("00", "Success"));
    }
}
