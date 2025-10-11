using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.VNPay.Queries;

public sealed record VNPayReturnView(string HtmlContent, bool SignatureValid, bool IsSuccess, string ResponseCode, string? TransactionReference);

public sealed record GetVNPayReturnViewQuery(
    IReadOnlyDictionary<string, string> QueryParameters,
    VNPayConfiguration Configuration) : IQuery<VNPayReturnView>;

internal sealed class GetVNPayReturnViewQueryHandler : IQueryHandler<GetVNPayReturnViewQuery, VNPayReturnView>
{
    private readonly ILogger<GetVNPayReturnViewQueryHandler> _logger;

    public GetVNPayReturnViewQueryHandler(ILogger<GetVNPayReturnViewQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<VNPayReturnView>> Handle(GetVNPayReturnViewQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Configuration.HashSecret))
        {
            return Task.FromResult(Result.Failure<VNPayReturnView>(VnPayErrors.ConfigurationMissing));
        }

        var query = new Dictionary<string, string>(request.QueryParameters, StringComparer.Ordinal);
        query.TryGetValue("vnp_SecureHash", out var secureHash);

        var filtered = query
            .Where(kvp => !string.Equals(kvp.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

        var dataToVerify = VnPayHelper.BuildDataToVerify(filtered);
        var calculatedHash = VnPayHelper.HmacSha512(request.Configuration.HashSecret, dataToVerify);
        _logger.LogInformation(
            "VNPay return signature verification - data: {Data}, calculated hash: {CalculatedHash}, provided hash: {ProvidedHash}",
            dataToVerify,
            calculatedHash,
            secureHash);

        var isSignatureValid = !string.IsNullOrWhiteSpace(secureHash) &&
                               string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);

        var responseCode = query.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        var isSuccess = isSignatureValid && string.Equals(responseCode, "00", StringComparison.Ordinal);
        var txnRef = query.TryGetValue("vnp_TxnRef", out var refValue) ? refValue : string.Empty;
        var amount = query.TryGetValue("vnp_Amount", out var amountValue) ? amountValue : string.Empty;

        var title = isSuccess ? "Payment Successful" : "Payment Failed";
        var statusMessage = isSignatureValid
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
        builder.AppendLine("            <dt>Response Code:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(responseCode))
            .AppendLine("</dd>");
        builder.AppendLine("            <dt>Signature Valid:</dt>");
        builder.Append("            <dd>")
            .Append(isSignatureValid)
            .AppendLine("</dd>");
        builder.AppendLine("        </dl>");
        builder.AppendLine("    </div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        var html = builder.ToString();

        var view = new VNPayReturnView(html, isSignatureValid, isSuccess, responseCode, txnRef);
        return Task.FromResult(Result.Success(view));
    }
}
