using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Application.Payments.VNPay;

internal static class VnPayHelper
{
    public static string HmacSha512(string secret, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string BuildDataToSign(IDictionary<string, string> parameters)
    {
        return string.Join("&",
            parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => string.Concat(kv.Key, "=", Uri.EscapeDataString(kv.Value))));
    }

    public static string BuildDataToVerify(IDictionary<string, string> parameters)
    {
        return string.Join("&",
            parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => string.Concat(kv.Key, "=", WebUtility.UrlEncode(kv.Value))));
    }

    public static string CreatePaymentUrl(string baseUrl, string hashSecret, IDictionary<string, string> parameters)
    {
        var sanitized = parameters
            .Where(kv => !string.IsNullOrEmpty(kv.Value)
                      && !kv.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                      && !kv.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

        var dataToSign = BuildDataToSign(sanitized);
        var secureHash = HmacSha512(hashSecret, dataToSign);

        var sep = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return string.Concat(baseUrl, sep, dataToSign, "&vnp_SecureHashType=HMACSHA512&vnp_SecureHash=", secureHash);
    }

    public static TimeZoneInfo GetVietnamTimeZone()
    {
        var ids = new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh" };
        foreach (var id in ids)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
