using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachCertifications.Handler;
using Application.CoachMedia.Handler;
using Application.Features;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

internal static class CoachProfileFileUrlHelper
{
    public static async Task<CoachProfileDto> WithSignedUrlsAsync(
        CoachProfileDto dto,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        var signedProfile = await CoachProfileAvatarHelper
            .WithSignedAvatarAsync(dto, storage, cancellationToken)
            .ConfigureAwait(false);

        var signedMedia = await CoachMediaFileUrlHelper
            .WithSignedFileUrlsAsync(ToReadOnlyList(signedProfile.Media), storage, cancellationToken)
            .ConfigureAwait(false);

        var signedCertifications = await CoachCertificationFileUrlHelper
            .WithSignedFileUrlsAsync(ToReadOnlyList(signedProfile.Certifications), storage, cancellationToken)
            .ConfigureAwait(false);

        return signedProfile with
        {
            Media = signedMedia,
            Certifications = signedCertifications
        };
    }

    public static async Task<IReadOnlyList<CoachProfileDto>> WithSignedUrlsAsync(
        IReadOnlyList<CoachProfileDto> dtos,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
        {
            return dtos;
        }

        var result = new CoachProfileDto[dtos.Count];
        for (var i = 0; i < dtos.Count; i++)
        {
            result[i] = await WithSignedUrlsAsync(dtos[i], storage, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private static IReadOnlyList<T> ToReadOnlyList<T>(IReadOnlyCollection<T> source)
    {
        if (source is IReadOnlyList<T> list)
        {
            return list;
        }

        return source.ToArray();
    }
}
