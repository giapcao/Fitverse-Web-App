using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using SharedLibrary.Storage;

namespace Application.Users.Handler;

internal static class UserAvatarHelper
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(15);

    public static bool TryGetStorageKey(string? value, out string key)
    {
        key = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var path = uri.AbsolutePath.TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (!path.Contains('/'))
            {
                key = path;
                return true;
            }

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return false;
            }

            var candidate = path;
            if (segments.Length > 1)
            {
                // For path-style S3 endpoints the first segment represents the bucket.
                var skipFirst = string.Join('/', segments.Skip(1));
                if (!string.IsNullOrWhiteSpace(skipFirst) && !uri.Host.Contains('.', StringComparison.Ordinal))
                {
                    candidate = skipFirst;
                }
            }

            candidate = candidate.Trim();
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                key = candidate;
                return true;
            }

            return false;
        }

        key = trimmed;
        return true;
    }

    public static async Task<UserDto> WithSignedAvatarAsync(UserDto dto, IFileStorageService storage, CancellationToken cancellationToken)
    {
        if (!TryGetStorageKey(dto.AvatarUrl, out var key))
        {
            return dto;
        }

        var urls = await storage.GetFileUrlAsync(key, DefaultExpiry, cancellationToken).ConfigureAwait(false);
        return dto with { AvatarUrl = urls.InlineUrl };
    }

    public static async Task<IReadOnlyList<UserDto>> WithSignedAvatarsAsync(IEnumerable<UserDto> dtos, IFileStorageService storage, CancellationToken cancellationToken)
    {
        var items = dtos?.ToArray() ?? Array.Empty<UserDto>();
        if (items.Length == 0)
        {
            return items;
        }

        for (var i = 0; i < items.Length; i++)
        {
            items[i] = await WithSignedAvatarAsync(items[i], storage, cancellationToken).ConfigureAwait(false);
        }

        return items;
    }
}
