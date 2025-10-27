using Application.Payments.Common;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Application.Payments.Returns;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Net.payOS.Types;
using SharedLibrary.Contracts.Payments;

namespace Application.PayOs.Returns;

internal sealed class PayOsReturnHandler
    : PaymentGatewayReturnHandlerBase<PayOsConfiguration>
{
    public PayOsReturnHandler(
        IPaymentRepository paymentRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        ILogger<PayOsReturnHandler> logger)
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

    public override Gateway Gateway => Gateway.Payos;

    protected override bool IsConfigurationValid(PayOsConfiguration configuration)
    {
        return configuration.HasCredentials;
    }

    protected override Error GetConfigurationError() => PayOsErrors.ConfigurationMissing;

    protected override bool ValidateSignature(
        IReadOnlyDictionary<string, string> parameters,
        PayOsConfiguration configuration)
    {
        if (!parameters.TryGetValue("orderCode", out var orderCodeValue) ||
            !long.TryParse(orderCodeValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var orderCode))
        {
            return false;
        }

        try
        {
            var client = configuration.CreateClient();
            var info = client.getPaymentLinkInformation(orderCode).GetAwaiter().GetResult();
            if (info is null)
            {
                return false;
            }

            if (parameters is IDictionary<string, string> dict)
            {
                dict["__payos_amount"] = info.amount.ToString(CultureInfo.InvariantCulture);
                dict["__payos_status"] = info.status ?? string.Empty;
                dict["__payos_paymentLinkId"] = info.id ?? string.Empty;
                dict["__payos_orderCode"] = info.orderCode.ToString(CultureInfo.InvariantCulture);

                var transaction = info.transactions?.FirstOrDefault();
                if (transaction != null)
                {
                    dict["__payos_reference"] = transaction.reference ?? string.Empty;
                    dict["__payos_transactionAmount"] =
                        transaction.amount.ToString(CultureInfo.InvariantCulture);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    protected override bool TryResolvePaymentId(
        IReadOnlyDictionary<string, string> parameters,
        out Guid paymentId)
    {
        if (parameters.TryGetValue("paymentId", out var paymentIdValue) &&
            Guid.TryParse(paymentIdValue, out paymentId))
        {
            return true;
        }

        paymentId = Guid.Empty;
        return false;
    }

    protected override bool TryResolveAmount(
        IReadOnlyDictionary<string, string> parameters,
        out long amountVnd)
    {
        if (parameters.TryGetValue("__payos_amount", out var amount) &&
            long.TryParse(amount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var normalized))
        {
            amountVnd = normalized;
            return true;
        }

        if (parameters.TryGetValue("__payos_transactionAmount", out var transactionAmount) &&
            long.TryParse(transactionAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var txAmount))
        {
            amountVnd = txAmount;
            return true;
        }

        amountVnd = 0;
        return false;
    }

    protected override string ResolveResponseCode(IReadOnlyDictionary<string, string> parameters)
    {
        return parameters.TryGetValue("code", out var code) ? code : string.Empty;
    }

    protected override string ResolveTransactionStatus(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("__payos_status", out var status) && !string.IsNullOrWhiteSpace(status))
        {
            return status;
        }

        return parameters.TryGetValue("status", out var rawStatus) ? rawStatus : string.Empty;
    }

    protected override bool IsSuccess(string responseCode, string transactionStatus)
    {
        return string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(transactionStatus, "PAID", StringComparison.OrdinalIgnoreCase);
    }

    protected override string? ResolveGatewayTransactionId(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("__payos_reference", out var reference) &&
            !string.IsNullOrWhiteSpace(reference))
        {
            return reference;
        }

        if (parameters.TryGetValue("id", out var id) && !string.IsNullOrWhiteSpace(id))
        {
            return id;
        }

        if (parameters.TryGetValue("__payos_paymentLinkId", out var paymentLinkId) &&
            !string.IsNullOrWhiteSpace(paymentLinkId))
        {
            return paymentLinkId;
        }

        return null;
    }

    protected override string GetLedgerEntryDescription(Domain.Entities.WalletJournal journal) =>
        $"PayOS payment {journal.PaymentId}";
}
