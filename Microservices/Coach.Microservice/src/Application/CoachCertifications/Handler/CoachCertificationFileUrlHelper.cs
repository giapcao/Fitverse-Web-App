using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Features;
using SharedLibrary.Storage;

namespace Application.CoachCertifications.Handler;

internal static class CoachCertificationFileUrlHelper
{
    private static readonly TimeSpan SignedUrlLifetime = TimeSpan.FromHours(1);

    public static async Task<CoachCertificationDto> WithSignedFileUrlAsync(
        CoachCertificationDto dto,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (!StorageUrlResolver.TryGetSignableInput(dto.FileUrl, out var signable))
        {
            var downloadUrl = dto.FileDownloadUrl ?? dto.FileUrl;
            return dto with { FileDownloadUrl = downloadUrl };
        }

        var signedUrls = await fileStorageService
            .GetFileUrlAsync(signable, SignedUrlLifetime, cancellationToken)
            .ConfigureAwait(false);

        return dto with
        {
            FileUrl = signedUrls.InlineUrl,
            FileDownloadUrl = signedUrls.DownloadUrl
        };
    }

    public static async Task<IReadOnlyList<CoachCertificationDto>> WithSignedFileUrlsAsync(
        IReadOnlyList<CoachCertificationDto> dtos,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
        {
            return dtos;
        }

        var result = new CoachCertificationDto[dtos.Count];
        for (var i = 0; i < dtos.Count; i++)
        {
            result[i] = await WithSignedFileUrlAsync(dtos[i], fileStorageService, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
