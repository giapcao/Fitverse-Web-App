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
    private const string DefaultAvatarPublicUrl = "https://fitverse-prod-user-content.s3.ap-southeast-1.amazonaws.com/default_avt.jpg";
    private static readonly TimeSpan SignedUrlLifetime = TimeSpan.FromHours(1);

    public static async Task<CoachProfileDto> WithSignedAvatarAsync(CoachProfileDto dto, IFileStorageService storage, CancellationToken cancellationToken)
    {
        var urls = await ResolveAvatarUrlsAsync(dto.AvatarUrl, dto.AvatarDownloadUrl, storage, cancellationToken).ConfigureAwait(false);
        return dto with { AvatarUrl = urls.Inline, AvatarDownloadUrl = urls.Download };
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
        var urls = await ResolveAvatarUrlsAsync(dto.AvatarUrl, dto.AvatarDownloadUrl, storage, cancellationToken).ConfigureAwait(false);
        return dto with { AvatarUrl = urls.Inline, AvatarDownloadUrl = urls.Download };
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

    private static async Task<(string Inline, string Download)> ResolveAvatarUrlsAsync(
        string? avatarUrl,
        string? avatarDownloadUrl,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        var effectiveAvatar = string.IsNullOrWhiteSpace(avatarUrl)
            ? DefaultAvatar
            : avatarUrl.Trim();

        if (string.Equals(effectiveAvatar, DefaultAvatar, StringComparison.OrdinalIgnoreCase))
        {
            effectiveAvatar = DefaultAvatarPublicUrl;
        }

        if (!StorageUrlResolver.TryGetSignableInput(effectiveAvatar, out var signable))
        {
            var fallbackInline = string.IsNullOrWhiteSpace(avatarUrl) ||
                                 string.Equals(avatarUrl.Trim(), DefaultAvatar, StringComparison.OrdinalIgnoreCase)
                ? DefaultAvatarPublicUrl
                : effectiveAvatar;

            var fallbackDownload = string.IsNullOrWhiteSpace(avatarDownloadUrl)
                ? fallbackInline
                : avatarDownloadUrl;

            return (fallbackInline, fallbackDownload);
        }

        var signedUrls = await storage.GetFileUrlAsync(signable, SignedUrlLifetime, cancellationToken).ConfigureAwait(false);
        return (signedUrls.InlineUrl, signedUrls.DownloadUrl);
    }
}
