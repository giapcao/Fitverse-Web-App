using System.Net;
using System.Text;
using System.Text.Json;
using Application.Momo;
using Application.Payments;
using Application.Payments.Common;
using Application.Payments.Queries;
using Application.Payments.Returns;
using Application.Payments.VNPay;
using Application.VNPay;
using Domain.Enums;
using SharedLibrary.Common.ResponseModel;
using WebApi.Options;

namespace WebApi.Helper;

public static class PaymentsHelpers
{
    public static MomoConfiguration BuildMomoConfiguration(MomoOptions value)
    {
        return new MomoConfiguration(
            value.PartnerCode,
            value.AccessKey,
            value.SecretKey,
            value.CreatePaymentUrl,
            value.RedirectWalletUrl,
            value.RedirectBookingUrl,
            value.IpnUrl,
            string.IsNullOrWhiteSpace(value.RequestType) ? "captureWallet" : value.RequestType,
            string.IsNullOrWhiteSpace(value.Lang) ? "vi" : value.Lang,
            value.AutoCapture,
            string.IsNullOrWhiteSpace(value.PartnerName) ? null : value.PartnerName,
            string.IsNullOrWhiteSpace(value.StoreId) ? null : value.StoreId,
            string.IsNullOrWhiteSpace(value.ExtraDataTemplate) ? null : value.ExtraDataTemplate);
    }

    public static VnPayConfiguration BuildVnPayConfiguration(VNPayOptions value)
    {
        return new VnPayConfiguration(
            value.TmnCode,
            value.HashSecret,
            value.BaseUrl,
            value.ReturnWalletUrl,
            value.ReturnBookingUrl,
            value.IpnUrl);
    }

    public static string BuildMomoOrderInfo(PaymentResponse payment, PaymentFlow flow)
    {
        var prefix = flow switch
        {
            PaymentFlow.DepositWallet => "Deposit wallet",
            PaymentFlow.PayoutWallet => "Wallet payout",
            PaymentFlow.Booking => "Booking payment",
            PaymentFlow.BookingByWallet => "Wallet booking hold",
            _ => "Payment"
        };

        if (payment.BookingId != Guid.Empty)
        {
            return $"{prefix} - booking {payment.BookingId}";
        }

        return $"{prefix} - payment {payment.Id}";
    }

    public static string BuildMomoExtraData(PaymentResponse payment, Guid userId, Guid? walletId)
    {
        var payload = new Dictionary<string, string>
        {
            ["paymentId"] = payment.Id.ToString(),
            ["userId"] = userId.ToString()
        };

        if (walletId.HasValue)
        {
            payload["walletId"] = walletId.Value.ToString();
        }

        if (payment.BookingId != Guid.Empty)
        {
            payload["bookingId"] = payment.BookingId.ToString();
        }

        var json = JsonSerializer.Serialize(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static bool TryResolveGatewayContext(
        Dictionary<string, string> parameters,
        VNPayOptions vnPayOptions,
        MomoOptions momoOptions,
        out Gateway gateway,
        out IPaymentGatewayConfiguration configuration,
        out Guid? userId,
        out Error? error)
    {
        var resolvedGateway = ResolveGatewayFromParameters(parameters);
        if (!resolvedGateway.HasValue)
        {
            gateway = default;
            configuration = default!;
            userId = null;
            error = new Error("Payment.GatewayUnknown", "Unable to determine payment gateway from return payload.");
            return false;
        }

        gateway = resolvedGateway.Value;
        error = null;

        switch (gateway)
        {
            case Gateway.Vnpay:
                configuration = BuildVnPayConfiguration(vnPayOptions);
                userId = ResolveUserIdParameter(parameters);
                return true;

            case Gateway.Momo:
                configuration = BuildMomoConfiguration(momoOptions);
                userId = null;
                return true;

            default:
                configuration = default!;
                userId = null;
                error = new Error(
                    "Payment.GatewayNotSupported",
                    $"Gateway '{gateway}' is not supported for payment returns.");
                return false;
        }
    }

    public static Gateway? ResolveGatewayFromParameters(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.Keys.Any(key => key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)))
        {
            return Gateway.Vnpay;
        }

        if (parameters.ContainsKey("partnerCode") ||
            parameters.ContainsKey("resultCode") ||
            parameters.ContainsKey("transId") ||
            (parameters.ContainsKey("orderId") && parameters.ContainsKey("signature")))
        {
            return Gateway.Momo;
        }

        return null;
    }

    public static Guid? ResolveUserIdParameter(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("userId", out var userIdValue) &&
            Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }

    public static string BuildMomoReturnHtml(
        IReadOnlyDictionary<string, string> parameters,
        PaymentGatewayReturnResult commandResult)
    {
        var isSuccess = commandResult.TransactionCaptured;
        var title = isSuccess ? "Payment Successful" : "Payment Failed";

        var responseCode = parameters.TryGetValue("resultCode", out var resultCode) ? resultCode : string.Empty;
        var amount = parameters.TryGetValue("amount", out var amountValue) ? amountValue : string.Empty;
        var orderId = parameters.TryGetValue("orderId", out var orderValue) ? orderValue : string.Empty;
        var transactionId = parameters.TryGetValue("transId", out var transId) ? transId : string.Empty;
        var message = parameters.TryGetValue("message", out var messageValue) && !string.IsNullOrWhiteSpace(messageValue)
            ? messageValue
            : (isSuccess ? "Your MoMo payment was recorded successfully." : "The payment was not completed.");

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
            .Append(WebUtility.HtmlEncode(message))
            .AppendLine("</p>");
        builder.AppendLine("        <dl>");
        builder.AppendLine("            <dt>Order Id:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(orderId))
            .AppendLine("</dd>");
        builder.AppendLine("            <dt>Amount:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(amount))
            .AppendLine("</dd>");
        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            builder.AppendLine("            <dt>Transaction Id:</dt>");
            builder.Append("            <dd>")
                .Append(WebUtility.HtmlEncode(transactionId))
                .AppendLine("</dd>");
        }
        if (commandResult.UserId.HasValue)
        {
            builder.AppendLine("            <dt>User Id:</dt>");
            builder.Append("            <dd>")
                .Append(WebUtility.HtmlEncode(commandResult.UserId.Value.ToString()))
                .AppendLine("</dd>");
        }
        builder.AppendLine("            <dt>Response Code:</dt>");
        builder.Append("            <dd>")
            .Append(WebUtility.HtmlEncode(responseCode))
            .AppendLine("</dd>");
        builder.AppendLine("            <dt>Gateway:</dt>");
        builder.Append("            <dd>MoMo</dd>");
        builder.AppendLine("        </dl>");
        builder.AppendLine("    </div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    public static string ResolveClientIp(HttpContext? context)
    {
        if (context is null)
        {
            return "127.0.0.1";
        }

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var realIp = forwarded
                    .ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp;
            }
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            return remoteIp.ToString();
        }

        return "127.0.0.1";
    }

    public static int ResolveErrorStatusCode(Error error)
    {
        return error.Code switch
        {
            var code when code == VnPayErrors.ConfigurationMissing.Code ||
                          code == MomoErrors.ConfigurationMissing.Code
                => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static bool TryResolvePaymentId(IReadOnlyDictionary<string, string> parameters, out Guid paymentId)
    {
        if (parameters.TryGetValue("vnp_TxnRef", out var txnRef) && Guid.TryParse(txnRef, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("vnp_OrderInfo", out var orderInfo))
        {
            var trailing = ExtractTrailingToken(orderInfo);
            if (!string.IsNullOrWhiteSpace(trailing) && Guid.TryParse(trailing, out paymentId))
            {
                return true;
            }
        }

        if (parameters.TryGetValue("orderId", out var orderId) && Guid.TryParse(orderId, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("paymentId", out var paymentIdValue) &&
            Guid.TryParse(paymentIdValue, out paymentId))
        {
            return true;
        }

        if (parameters.TryGetValue("extraData", out var extraData) &&
            TryExtractPaymentIdFromMomoExtra(extraData, out paymentId))
        {
            return true;
        }

        paymentId = Guid.Empty;
        return false;
    }

    public static string? ExtractTrailingToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value
            .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();
    }

    public static bool TryExtractPaymentIdFromMomoExtra(string extraData, out Guid paymentId)
    {
        paymentId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(extraData))
        {
            return false;
        }

        try
        {
            var decoded = Convert.FromBase64String(extraData);
            var json = Encoding.UTF8.GetString(decoded);
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("paymentId", out var paymentIdElement) &&
                Guid.TryParse(paymentIdElement.GetString(), out paymentId))
            {
                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }
}
