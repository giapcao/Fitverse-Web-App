using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Application.Payments.VNPay;

public static class VnPayHelper
{
    private const string SuccessCode = "00";
    private const string DefaultLocale = "vn";
    private const string DefaultCommand = "pay";
    private const string Currency = "VND";
    private const string OrderTypeOther = "other";
    private const string VietnamTimeFormat = "yyyyMMddHHmmss";
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    public static VnPaySignedRequest BuildPaymentUrl(
        VNPayConfiguration configuration,
        long amountVnd,
        string orderId,
        string clientIp,
        DateTime requestedAtUtc,
        Guid? walletId,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _ = walletId;

        var tmnCode = configuration.TmnCode?.Trim();
        var hashSecret = configuration.HashSecret?.Trim();
        var baseUrl = configuration.BaseUrl?.Trim();
        var returnUrl = configuration.ReturnUrl?.Trim();
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = AppendOrReplaceQueryParameter(returnUrl, "userId", userId.ToString());
        }

        if (string.IsNullOrWhiteSpace(tmnCode) ||
            string.IsNullOrWhiteSpace(hashSecret) ||
            string.IsNullOrWhiteSpace(baseUrl) ||
            string.IsNullOrWhiteSpace(returnUrl))
        {
            throw new ArgumentException("VNPay configuration is incomplete.");
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is not null");
        }
        
        if (amountVnd <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountVnd));
        }

        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("OrderId is required.", nameof(orderId));
        }

        var normalizedClientIp = string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp.Trim();

        var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Amount"] = FormatAmount(amountVnd),
            ["vnp_Command"] = DefaultCommand,
            ["vnp_CreateDate"] = FormatVietnamDate(requestedAtUtc),
            ["vnp_CurrCode"] = Currency,
            ["vnp_IpAddr"] = normalizedClientIp,
            ["vnp_Locale"] = DefaultLocale,
            ["vnp_OrderInfo"] = BuildOrderInfo(orderId),
            ["vnp_OrderType"] = OrderTypeOther,
            ["vnp_ReturnUrl"] = returnUrl ?? string.Empty,
            ["vnp_TmnCode"] = tmnCode,
            ["vnp_TxnRef"] = orderId,
            ["vnp_Version"] = "2.1.0"
        };

        var sortedParameters = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
        var rawQuery = BuildQueryString(sortedParameters);
        var secureHash = ComputeHmac(hashSecret, rawQuery); 

        var preferredOrder = new[]
        {
            "vnp_Amount",
            "vnp_Command",
            "vnp_CreateDate",
            "vnp_CurrCode",
            "vnp_IpAddr",
            "vnp_Locale",
            "vnp_OrderInfo",
            "vnp_OrderType",
            "vnp_ReturnUrl",
            "vnp_TmnCode",
            "vnp_TxnRef",
            "vnp_Version"
        };

        var orderedEntries = new List<KeyValuePair<string, string>>(sortedParameters.Count + 1);
        var handledKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in preferredOrder)
        {
            if (sortedParameters.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            {
                orderedEntries.Add(new KeyValuePair<string, string>(key, value));
                handledKeys.Add(key);
            }
        }

        foreach (var kvp in sortedParameters)
        {
            if (handledKeys.Add(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
            {
                orderedEntries.Add(kvp);
            }
        }

        orderedEntries.Add(new KeyValuePair<string, string>("vnp_SecureHash", secureHash));

        var signedQuery = BuildQueryString(orderedEntries);
        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        var paymentUrl = $"{baseUrl}{separator}{signedQuery}";

        return new VnPaySignedRequest(paymentUrl, rawQuery, secureHash);
    }

    public static bool ValidateSignature(IReadOnlyDictionary<string, string> parameters, string hashSecret)
    {
        if (string.IsNullOrWhiteSpace(hashSecret) ||
            !parameters.TryGetValue("vnp_SecureHash", out var actualSignature) ||
            string.IsNullOrWhiteSpace(actualSignature))
        {
            return false;
        }

        var normalizedSecret = hashSecret.Trim();
        if (string.IsNullOrEmpty(normalizedSecret))
        {
            return false;
        }

        var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kvp in parameters)
        {
            if (string.IsNullOrEmpty(kvp.Value))
            {
                continue;
            }

            if (!kvp.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (kvp.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            sorted[kvp.Key] = kvp.Value;
        }

        var rawQuery = BuildQueryString(sorted);
        var expectedSignature = ComputeHmac(normalizedSecret, rawQuery);
        return actualSignature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSuccess(string? responseCode, string? transactionStatus)
    {
        var responseOk = string.Equals(responseCode, SuccessCode, StringComparison.OrdinalIgnoreCase);
        if (!responseOk)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(transactionStatus))
        {
            return responseOk;
        }

        return string.Equals(transactionStatus, SuccessCode, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatAmount(long amountVnd)
    {
        return (amountVnd * 100).ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatVietnamDate(DateTime requestedAtUtc)
    {
        var utc = requestedAtUtc.Kind switch
        {
            DateTimeKind.Utc => requestedAtUtc,
            DateTimeKind.Local => requestedAtUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(requestedAtUtc, DateTimeKind.Utc)
        };

        var vietnamTime = utc + VietnamOffset;
        return vietnamTime.ToString(VietnamTimeFormat, CultureInfo.InvariantCulture);
    }

    private static string BuildOrderInfo(string orderId)
    {
        return $"Thanh toan don hang :{orderId}";
    }

    private static string UrlEncodeUpper(string value)
    {
        var enc = HttpUtility.UrlEncode(value ?? string.Empty, Encoding.UTF8) ?? string.Empty;
        return Regex.Replace(enc, "%[0-9a-fA-F]{2}", m => m.Value.ToUpperInvariant());
    }

    private static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        return string.Join("&", parameters
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .Select(kvp => $"{kvp.Key}={UrlEncodeUpper(kvp.Value)}"));
    }

    private static string ComputeHmac(string secret, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash); 
    }

    private static string? AppendOrReplaceQueryParameter(string? url, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        if (string.IsNullOrEmpty(value))
        {
            return url;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            var builder = new UriBuilder(absoluteUri);
            var query = HttpUtility.ParseQueryString(builder.Query ?? string.Empty);
            query[key] = value;
            builder.Query = query.ToString();
            return builder.Uri.ToString();
        }

        var separator = url.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{url}{separator}{key}={UrlEncodeUpper(value)}";
    }
}

public sealed record VnPaySignedRequest(string PaymentUrl, string RawQuery, string SecureHash);
