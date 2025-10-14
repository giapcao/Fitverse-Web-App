using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

internal static class CoachProfileAvatarHelper
{
    public const string DefaultAvatar = "default_avt.jpg";
    private static readonly TimeSpan SignedUrlLifetime = TimeSpan.FromHours(1);

    private static bool ShouldSign(string? url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && !url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(url, DefaultAvatar, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<CoachProfileDto> WithSignedAvatarAsync(CoachProfileDto dto, IFileStorageService storage, CancellationToken cancellationToken)
    {
        if (!ShouldSign(dto.AvatarUrl))
        {
            return dto;
        }

        var signedUrl = await storage.GetFileUrlAsync(dto.AvatarUrl!, SignedUrlLifetime, cancellationToken).ConfigureAwait(false);
        return dto with { AvatarUrl = signedUrl };
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
        if (!ShouldSign(dto.AvatarUrl))
        {
            return dto;
        }

        var signedUrl = await storage.GetFileUrlAsync(dto.AvatarUrl!, SignedUrlLifetime, cancellationToken).ConfigureAwait(false);
        return dto with { AvatarUrl = signedUrl };
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
