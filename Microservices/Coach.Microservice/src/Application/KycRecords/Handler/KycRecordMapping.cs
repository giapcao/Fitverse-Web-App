using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.CoachProfiles.Handler;
using Application.Features;
using Domain.Persistence.Models;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

internal static class KycRecordMapping
{
    public static KycRecordDto ToDto(KycRecord record)
    {
        return new KycRecordDto(
            record.Id,
            record.CoachId,
            record.IdDocumentUrl,
            record.AdminNote,
            record.Status,
            record.SubmittedAt,
            record.ReviewedAt,
            record.ReviewerId,
            record.Coach is null ? null : CoachProfileMapping.ToSummaryDto(record.Coach));
    }

    public static async Task<KycRecordDto> ToDtoWithSignedCoachAsync(
        KycRecord record,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        var dto = ToDto(record);
        if (dto.Coach is null)
        {
            return dto;
        }

        var signedCoach = await CoachProfileAvatarHelper
            .WithSignedAvatarAsync(dto.Coach, storage, cancellationToken)
            .ConfigureAwait(false);

        return dto with { Coach = signedCoach };
    }

    public static async Task<IReadOnlyList<KycRecordDto>> ToDtoListWithSignedCoachAsync(
        IEnumerable<KycRecord> records,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        var result = new List<KycRecordDto>();
        foreach (var record in records)
        {
            result.Add(await ToDtoWithSignedCoachAsync(record, storage, cancellationToken).ConfigureAwait(false));
        }

        return result;
    }
}
