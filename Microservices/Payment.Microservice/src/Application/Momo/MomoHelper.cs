using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Application.Momo;

internal static class MomoHelper
{
    private static readonly string[] ReservedSignatureKeys =
    [
        "signature"
    ];

    public static string BuildRawSignature(
        string accessKey,
        long amount,
        string extraData,
        string ipnUrl,
        string orderId,
        string orderInfo,
        string partnerCode,
        string redirectUrl,
        string requestId,
        string requestType)
    {
        return $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
    }

    public static string BuildReturnSignaturePayload(IReadOnlyDictionary<string, string> parameters)
    {
        var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in parameters)
        {
            if (ReservedSignatureKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            sorted[key] = value ?? string.Empty;
        }

        return string.Join("&", sorted.Select(pair => $"{pair.Key}={pair.Value}"));
    }

    public static string ComputeSignature(string rawData, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string GenerateRequestId(Guid paymentId) =>
        $"{paymentId:N}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

    public static bool TryDecodeExtraData(string? base64, out Dictionary<string, string> values)
    {
        values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        try
        {
            var decoded = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(decoded);
            using var document = JsonDocument.Parse(json);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                values[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True or JsonValueKind.False => property.Value.GetBoolean().ToString(),
                    _ => property.Value.GetRawText()
                };
            }

            return true;
        }
        catch
        {
            values.Clear();
            return false;
        }
    }
}
