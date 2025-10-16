using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

internal static class CoachProfileAvatarHelper
{
    public const string DefaultAvatar = "default_avt.jpg";
    private static readonly TimeSpan SignedUrlLifetime = TimeSpan.FromHours(1);

    public static async Task<CoachProfileDto> WithSignedAvatarAsync(CoachProfileDto dto, IFileStorageService storage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.AvatarUrl) || string.Equals(dto.AvatarUrl, DefaultAvatar, StringComparison.OrdinalIgnoreCase))
        {
            var downloadUrl = dto.AvatarDownloadUrl ?? dto.AvatarUrl;
            return dto with { AvatarDownloadUrl = downloadUrl };
        }

        if (!StorageUrlResolver.TryGetSignableInput(dto.AvatarUrl, out var signable))
        {
            var downloadUrl = dto.AvatarDownloadUrl ?? dto.AvatarUrl;
            return dto with { AvatarDownloadUrl = downloadUrl };
        }

        var signedUrls = await storage.GetFileUrlAsync(signable, SignedUrlLifetime, cancellationToken).ConfigureAwait(false);
        return dto with
        {
            AvatarUrl = signedUrls.InlineUrl,
            AvatarDownloadUrl = signedUrls.DownloadUrl
        };
    }

    public static async Task<IReadOnlyList<CoachProfileDto>> WithSignedAvatarsAsync(IReadOnlyList<CoachProfileDto> dtos, IFileStorageService storage, CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
        {
            return dtos;
        }

        var result = new CoachProfileDto[dtos.Count];
        for (var i = 0; i < dtos.Count; i++)
        {
            result[i] = await WithSignedAvatarAsync(dtos[i], storage, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    public static async Task<CoachProfileSummaryDto> WithSignedAvatarAsync(CoachProfileSummaryDto dto, IFileStorageService storage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.AvatarUrl) || string.Equals(dto.AvatarUrl, DefaultAvatar, StringComparison.OrdinalIgnoreCase))
        {
            var downloadUrl = dto.AvatarDownloadUrl ?? dto.AvatarUrl;
            return dto with { AvatarDownloadUrl = downloadUrl };
        }

        if (!StorageUrlResolver.TryGetSignableInput(dto.AvatarUrl, out var signable))
        {
            var downloadUrl = dto.AvatarDownloadUrl ?? dto.AvatarUrl;
            return dto with { AvatarDownloadUrl = downloadUrl };
        }

        var signedUrls = await storage.GetFileUrlAsync(signable, SignedUrlLifetime, cancellationToken).ConfigureAwait(false);
        return dto with
        {
            AvatarUrl = signedUrls.InlineUrl,
            AvatarDownloadUrl = signedUrls.DownloadUrl
        };
    }

    public static async Task<IReadOnlyList<CoachProfileSummaryDto>> WithSignedAvatarsAsync(IReadOnlyList<CoachProfileSummaryDto> dtos, IFileStorageService storage, CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
        {
            return dtos;
        }

        var result = new CoachProfileSummaryDto[dtos.Count];
        for (var i = 0; i < dtos.Count; i++)
        {
            result[i] = await WithSignedAvatarAsync(dtos[i], storage, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
