using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Application.Abstractions.Messaging;
using Application.VNPay;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.VNPay.Queries;

public sealed record VNPayReturnView(
    string HtmlContent,
    bool SignatureValid,
    bool IsSuccess,
    string ResponseCode,
    string? TransactionReference,
    Guid? UserId);

public sealed record GetVNPayReturnViewQuery(
    IReadOnlyDictionary<string, string> QueryParameters,
    VnPayConfiguration Configuration,
    Guid? UserIdOverride) : IQuery<VNPayReturnView>;

internal sealed class GetVnPayReturnViewQueryHandler : IQueryHandler<GetVNPayReturnViewQuery, VNPayReturnView>
{
    private readonly ILogger<GetVnPayReturnViewQueryHandler> _logger;

    public GetVnPayReturnViewQueryHandler(ILogger<GetVnPayReturnViewQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<VNPayReturnView>> Handle(GetVNPayReturnViewQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Configuration.HashSecret))
        {
            return Task.FromResult(Result.Failure<VNPayReturnView>(VnPayErrors.ConfigurationMissing));
        }

        var parameters = new Dictionary<string, string>(request.QueryParameters, StringComparer.OrdinalIgnoreCase);
        var signatureValid = VnPayHelper.ValidateSignature(parameters, request.Configuration.HashSecret);
        if (!signatureValid)
        {
            _logger.LogWarning("VNPay return signature failed validation for payload {@Payload}", parameters);
        }

        var responseCode = parameters.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        var transactionStatus = parameters.TryGetValue("vnp_TransactionStatus", out var status) ? status : string.Empty;
        var amount = parameters.TryGetValue("vnp_Amount", out var amountValue) ? amountValue : string.Empty;

        var txnRef = ResolveTransactionReference(parameters);
        var userId = request.UserIdOverride ?? ResolveUserId(parameters);
        var isSuccess = signatureValid && VnPayHelper.IsSuccess(responseCode, transactionStatus);

        var title = isSuccess ? "Payment Successful" : "Payment Failed";
        var statusMessage = signatureValid
            ? (isSuccess ? "Your payment was recorded successfully." : "The payment was not completed.")
            : "Signature validation failed.";

        var builder = new StringBuilder();

        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("    <meta charset=\"utf-8\" />");
        builder.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        builder.Append("    <title>")
            .Append(WebUtility.HtmlEncode(title))
            .AppendLine("</title>");
        builder.AppendLine("    <style>");
        builder.AppendLine("        body { font-family: Arial, sans-serif; margin: 2rem; color: #333; }");
        builder.AppendLine($"        .status {{ padding: 1.5rem; border-radius: 8px; background-color: {(isSuccess ? "#e6ffed" : "#ffecec")}; border: 1px solid {(isSuccess ? "#28a745" : "#dc3545")}; }}");
        builder.AppendLine("        h1 { margin-top: 0; }");
        builder.AppendLine("        dl { display: grid; grid-template-columns: max-content 1fr; gap: 0.5rem 1rem; }");
        builder.AppendLine("        dt { font-weight: bold; }");
        builder.AppendLine("    </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("    <div class=\"status\">");
        builder.Append("        <h1>")
            .Append(WebUtility.HtmlEncode(title))
            .AppendLine("</h1>");
        builder.Append("        <p>")
            .Append(WebUtility.HtmlEncode(statusMessage))
            .AppendLine("</p>");
        builder.AppendLine("        <dl>");
        builder.AppendLine("            <dt>Order Id:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(txnRef))
            .AppendLine("</dd>");
        builder.AppendLine("            <dt>Amount:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(amount))
            .AppendLine("</dd>");
        if (userId.HasValue)
        {
            builder.AppendLine("            <dt>User Id:</dt>");
            builder.Append("            <dd>")
                .Append(WebUtility.HtmlEncode(userId.Value.ToString()))
                .AppendLine("</dd>");
        }
        builder.AppendLine("            <dt>Response Code:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(responseCode))
            .AppendLine("</dd>");
        builder.AppendLine("            <dt>Signature Valid:</dt>");
        builder.Append("            <dd>")
            .Append(signatureValid)
            .AppendLine("</dd>");
        builder.AppendLine("        </dl>");
        builder.AppendLine("    </div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        var html = builder.ToString();
        var view = new VNPayReturnView(html, signatureValid, isSuccess, responseCode, txnRef, userId);

        return Task.FromResult(Result.Success(view));
    }

    private static string ResolveTransactionReference(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("vnp_TxnRef", out var txnRef) && !string.IsNullOrWhiteSpace(txnRef))
        {
            return txnRef;
        }

        if (parameters.TryGetValue("vnp_OrderInfo", out var orderInfo) && !string.IsNullOrWhiteSpace(orderInfo))
        {
            var token = orderInfo
                .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .LastOrDefault();

            if (!string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            return orderInfo;
        }

        return string.Empty;
    }

    private static Guid? ResolveUserId(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("userId", out var userIdValue) &&
            Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }
}
