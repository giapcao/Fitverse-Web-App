using System.Collections.Generic;
using System.Globalization;
using Application.Momo;
using Application.Payments.Common;
using Application.Payments.Returns;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.Momo.Returns;

internal sealed class MomoReturnHandler : PaymentGatewayReturnHandlerBase<MomoConfiguration>
{
    public MomoReturnHandler(
        IPaymentRepository paymentRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletRepository walletRepository,
        ILogger<MomoReturnHandler> logger,
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

    public override Gateway Gateway => Gateway.Momo;

    protected override bool IsConfigurationValid(MomoConfiguration configuration)
    {
        return !string.IsNullOrWhiteSpace(configuration.PartnerCode) &&
               !string.IsNullOrWhiteSpace(configuration.AccessKey) &&
               !string.IsNullOrWhiteSpace(configuration.SecretKey);
    }

    protected override Error GetConfigurationError() => MomoErrors.ConfigurationMissing;

    protected override bool ValidateSignature(
        IReadOnlyDictionary<string, string> parameters,
        MomoConfiguration configuration)
    {
        if (!parameters.TryGetValue("signature", out var signature) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var signatureParameters = parameters;
        if (!parameters.TryGetValue("accessKey", out var accessKey) || string.IsNullOrWhiteSpace(accessKey))
        {
            var adjusted = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase)
            {
                ["accessKey"] = configuration.AccessKey
            };

            signatureParameters = adjusted;
        }

        var payload = MomoHelper.BuildReturnSignaturePayload(signatureParameters);
        var expectedSignature = MomoHelper.ComputeSignature(payload, configuration.SecretKey);
        var isValid = string.Equals(signature, expectedSignature, StringComparison.OrdinalIgnoreCase);

        if (isValid && parameters is IDictionary<string, string> mutable)
        {
            PopulateExtraData(mutable);
        }

        return isValid;
    }

    protected override bool TryResolvePaymentId(
        IReadOnlyDictionary<string, string> parameters,
        out Guid paymentId)
    {
        if (parameters.TryGetValue("orderId", out var orderId) &&
            Guid.TryParse(orderId, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("paymentId", out var paymentIdValue) &&
            Guid.TryParse(paymentIdValue, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("extraData", out var extraData) &&
            TryResolvePaymentIdFromExtraData(extraData, out paymentId))
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
        if (parameters.TryGetValue("amount", out var rawAmount) &&
            long.TryParse(rawAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedAmount))
        {
            amountVnd = parsedAmount;
            return true;
        }

        amountVnd = 0;
        return false;
    }

    protected override string ResolveResponseCode(IReadOnlyDictionary<string, string> parameters)
    {
        return parameters.TryGetValue("resultCode", out var resultCode) ? resultCode : string.Empty;
    }

    protected override string ResolveTransactionStatus(IReadOnlyDictionary<string, string> parameters)
    {
        return parameters.TryGetValue("message", out var message) ? message : string.Empty;
    }

    protected override bool IsSuccess(string responseCode, string transactionStatus)
    {
        return string.Equals(responseCode, "0", StringComparison.OrdinalIgnoreCase);
    }

    protected override string? ResolveGatewayTransactionId(IReadOnlyDictionary<string, string> parameters)
    {
        return parameters.TryGetValue("transId", out var transactionId) && !string.IsNullOrWhiteSpace(transactionId)
            ? transactionId
            : null;
    }

    protected override string GetLedgerEntryDescription(Domain.Entities.WalletJournal journal) =>
        $"MoMo payment {journal.PaymentId}";

    private static void PopulateExtraData(IDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("extraData", out var extraData) ||
            string.IsNullOrWhiteSpace(extraData))
        {
            return;
        }

        if (!MomoHelper.TryDecodeExtraData(extraData, out var extraValues))
        {
            return;
        }

        foreach (var (key, value) in extraValues)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parameters[key] = value;
            }
        }
    }

    private static bool TryResolvePaymentIdFromExtraData(string extraData, out Guid paymentId)
    {
        paymentId = Guid.Empty;

        if (!MomoHelper.TryDecodeExtraData(extraData, out var values))
        {
            return false;
        }

        if (values.TryGetValue("paymentId", out var value) && Guid.TryParse(value, out paymentId))
        {
            return true;
        }

        return false;
    }
}
