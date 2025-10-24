using System;

namespace Application.Common;

/// <summary>
/// Utility helpers for working with storage-backed file URLs.
/// Determines when a value should be re-signed and extracts a suitable input for the storage service.
/// </summary>
internal static class StorageUrlResolver
{
    private static readonly string[] AwsPresignMarkers =
    {
        "X-Amz-Signature",
        "X-Amz-Credential",
        "X-Amz-Algorithm"
    };

    public static bool TryGetSignableInput(string? url, out string input)
    {
        input = string.Empty;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute))
        {
            input = trimmed;
            return true;
        }

        if (IsAwsStyleUri(absolute))
        {
            input = trimmed;
            return true;
        }

        return false;
    }

    private static bool IsAwsStyleUri(Uri uri)
    {
        if (uri.Host.Contains("amazonaws.com", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrEmpty(uri.Query))
        {
            return false;
        }

        foreach (var marker in AwsPresignMarkers)
        {
            if (uri.Query.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        // Generic check for any query parameter following the X-Amz-* convention (MinIO, custom endpoints, etc.)
        return uri.Query.IndexOf("X-Amz-", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
