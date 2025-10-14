using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        if (string.IsNullOrWhiteSpace(dto.FileUrl) || IsAbsoluteUrl(dto.FileUrl))
        {
            return dto;
        }

        var signedUrl = await fileStorageService
            .GetFileUrlAsync(dto.FileUrl, SignedUrlLifetime, cancellationToken)
            .ConfigureAwait(false);

        return dto with { FileUrl = signedUrl };
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

    private static bool IsAbsoluteUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
