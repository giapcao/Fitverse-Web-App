using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        if (string.IsNullOrWhiteSpace(dto.Url) || IsAbsoluteUrl(dto.Url))
        {
            return dto;
        }

        var signedUrl = await fileStorageService
            .GetFileUrlAsync(dto.Url, SignedUrlLifetime, cancellationToken)
            .ConfigureAwait(false);

        return dto with { Url = signedUrl };
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

    private static bool IsAbsoluteUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
