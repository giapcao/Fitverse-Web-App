using System.Collections.Generic;
using System.Globalization;
using SharedLibrary.Contracts.Payments;

namespace Application.PayOs;

public static class PayOsHelper
{
    public static long GenerateOrderCode(Guid paymentId)
    {
        var bytes = paymentId.ToByteArray();
        var high = BitConverter.ToInt64(bytes, 0);
        var low = BitConverter.ToInt64(bytes, 8);
        var combined = high ^ low;

        var value = combined == long.MinValue
            ? long.MaxValue
            : Math.Abs(combined);

        if (value == 0)
        {
            value = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        const long range = 900_000;
        var normalized = value % range;

        return normalized + 100_000;
    }

    public static string BuildDescription(Guid paymentId, long orderCode, PaymentFlow flow, Guid? bookingId)
    {
        var prefix = flow switch
        {
            PaymentFlow.DepositWallet => "DEP",
            PaymentFlow.PayoutWallet => "PAYOUT",
            PaymentFlow.Booking => "BOOK",
            PaymentFlow.BookingByWallet => "HOLD",
            _ => "PAY"
        };

        var orderFragment = Math.Abs(orderCode).ToString(CultureInfo.InvariantCulture);
        if (orderFragment.Length > 6)
        {
            orderFragment = orderFragment[^6..];
        }

        var description = $"{prefix}-{orderFragment}";

        if (bookingId.HasValue && bookingId.Value != Guid.Empty)
        {
            var bookingFragment = bookingId.Value.ToString("N")[..4].ToUpperInvariant();
            var candidate = $"{description}-{bookingFragment}";
            if (candidate.Length <= 25)
            {
                description = candidate;
            }
        }
        else
        {
            var paymentFragment = paymentId.ToString("N")[..4].ToUpperInvariant();
            var candidate = $"{description}-{paymentFragment}";
            if (candidate.Length <= 25)
            {
                description = candidate;
            }
        }

        return description.Length <= 25 ? description : description[..25];
    }

    public static string? ApplyTemplate(
        string? template,
        Guid paymentId,
        long orderCode,
        Guid? bookingId,
        Guid? walletId,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        var result = template
            .Replace("{paymentId}", paymentId.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{orderCode}", orderCode.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{userId}", userId == Guid.Empty ? string.Empty : userId.ToString(), StringComparison.OrdinalIgnoreCase);

        if (bookingId.HasValue)
        {
            result = result.Replace("{bookingId}", bookingId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            result = result.Replace("{bookingId}", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        if (walletId.HasValue)
        {
            result = result.Replace("{walletId}", walletId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            result = result.Replace("{walletId}", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        result = SetQueryParameter(result, "paymentId", paymentId.ToString());
        result = SetQueryParameter(result, "orderCode", orderCode.ToString(CultureInfo.InvariantCulture));

        if (userId != Guid.Empty)
        {
            result = SetQueryParameter(result, "userId", userId.ToString());
        }

        if (bookingId.HasValue)
        {
            result = SetQueryParameter(result, "bookingId", bookingId.Value.ToString());
        }

        if (walletId.HasValue)
        {
            result = SetQueryParameter(result, "walletId", walletId.Value.ToString());
        }

        return result;
    }

    private static string SetQueryParameter(string url, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(value))
        {
            return url;
        }

        var fragmentIndex = url.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? url[fragmentIndex..] : string.Empty;
        var baseUrl = fragmentIndex >= 0 ? url[..fragmentIndex] : url;

        var queryIndex = baseUrl.IndexOf('?');
        if (queryIndex < 0)
        {
            return $"{baseUrl}?{key}={Uri.EscapeDataString(value)}{fragment}";
        }

        var query = baseUrl[(queryIndex + 1)..];
        var segments = query.Length == 0
            ? new List<string>()
            : new List<string>(query.Split('&', StringSplitOptions.RemoveEmptyEntries));

        var updated = false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var equalsIndex = segment.IndexOf('=');
            var segmentKey = equalsIndex >= 0 ? segment[..equalsIndex] : segment;

            if (segmentKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                segments[i] = $"{key}={Uri.EscapeDataString(value)}";
                updated = true;
            }
        }

        if (!updated)
        {
            segments.Add($"{key}={Uri.EscapeDataString(value)}");
        }

        var newQuery = string.Join("&", segments);
        var prefix = baseUrl[..(queryIndex + 1)];
        return $"{prefix}{newQuery}{fragment}";
    }
}
