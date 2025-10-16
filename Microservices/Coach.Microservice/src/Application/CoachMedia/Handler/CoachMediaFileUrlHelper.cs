using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features;
using SharedLibrary.Storage;

namespace Application.CoachMedia.Handler;

internal static class CoachMediaFileUrlHelper
{
    private static readonly TimeSpan SignedUrlLifetime = TimeSpan.FromHours(1);

    public static async Task<CoachMediaDto> WithSignedFileUrlAsync(
        CoachMediaDto dto,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (!StorageUrlResolver.TryGetSignableInput(dto.Url, out var signable))
        {
            var downloadUrl = dto.DownloadUrl ?? dto.Url;
            return dto with { DownloadUrl = downloadUrl };
        }

        var signedUrls = await fileStorageService
            .GetFileUrlAsync(signable, SignedUrlLifetime, cancellationToken)
            .ConfigureAwait(false);

        return dto with
        {
            Url = signedUrls.InlineUrl,
            DownloadUrl = signedUrls.DownloadUrl
        };
    }

    public static async Task<IReadOnlyList<CoachMediaDto>> WithSignedFileUrlsAsync(
        IReadOnlyList<CoachMediaDto> dtos,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
        {
            return dtos;
        }

        var result = new CoachMediaDto[dtos.Count];
        for (var i = 0; i < dtos.Count; i++)
        {
            result[i] = await WithSignedFileUrlAsync(dtos[i], fileStorageService, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
