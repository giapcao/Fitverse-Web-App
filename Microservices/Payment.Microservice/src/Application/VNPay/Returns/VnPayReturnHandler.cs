using System.Globalization;
using Application.Payments.Returns;
using Application.Payments.VNPay;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.VNPay.Returns;

internal sealed class VnPayReturnHandler : PaymentGatewayReturnHandlerBase<VnPayConfiguration>
{
    public VnPayReturnHandler(
        IPaymentRepository paymentRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletRepository walletRepository,
        ILogger<VnPayReturnHandler> logger,
        IUnitOfWork unitOfWork)
        : base(
            paymentRepository,
            walletJournalRepository,
            walletLedgerEntryRepository,
            walletBalanceRepository,
            walletRepository,
            unitOfWork,
            logger)
    {
    }

    public override Gateway Gateway => Gateway.Vnpay;

    protected override bool IsConfigurationValid(VnPayConfiguration configuration)
    {
        return !string.IsNullOrWhiteSpace(configuration.HashSecret) &&
               !string.IsNullOrWhiteSpace(configuration.TmnCode);
    }

    protected override Error GetConfigurationError() => VnPayErrors.ConfigurationMissing;

    protected override bool ValidateSignature(
        IReadOnlyDictionary<string, string> parameters,
        VnPayConfiguration configuration)
    {
        return VnPayHelper.ValidateSignature(parameters, configuration.HashSecret);
    }

    protected override bool TryResolvePaymentId(
        IReadOnlyDictionary<string, string> parameters,
        out Guid paymentId)
    {
        if (parameters.TryGetValue("vnp_TxnRef", out var txnRef) && Guid.TryParse(txnRef, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("vnp_OrderInfo", out var orderInfo))
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

    protected override bool TryResolveAmount(
        IReadOnlyDictionary<string, string> parameters,
        out long amountVnd)
    {
        if (parameters.TryGetValue("vnp_Amount", out var rawAmount) &&
            long.TryParse(rawAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amountFromGateway) &&
            amountFromGateway % 100 == 0)
        {
            amountVnd = amountFromGateway / 100;
            return true;
        }

        amountVnd = 0;
        return false;
    }

    protected override string ResolveResponseCode(IReadOnlyDictionary<string, string> parameters)
    {
        return parameters.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
    }

    protected override string ResolveTransactionStatus(IReadOnlyDictionary<string, string> parameters)
    {
        return parameters.TryGetValue("vnp_TransactionStatus", out var status) ? status : string.Empty;
    }

    protected override bool IsSuccess(string responseCode, string transactionStatus)
    {
        return VnPayHelper.IsSuccess(responseCode, transactionStatus);
    }

    protected override string? ResolveGatewayTransactionId(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("vnp_TransactionNo", out var transactionNo) &&
            !string.IsNullOrWhiteSpace(transactionNo))
        {
            return transactionNo;
        }

        return null;
    }

    protected override string GetLedgerEntryDescription(Domain.Entities.WalletJournal journal) =>
        $"VNPay payment {journal.PaymentId}";

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