using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

internal static class KycRecordStatusUpdater
{
    public static async Task<Result<KycRecordDto>> UpdateAsync(
        IKycRecordRepository recordRepository,
        ICoachProfileRepository profileRepository,
        Guid recordId,
        KycStatus targetStatus,
        string? adminNote,
        Guid? reviewerId,
        CancellationToken cancellationToken)
    {
        var record = await recordRepository.GetDetailedByIdAsync(recordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.NotFound", $"KYC record {recordId} was not found."));
        }

        if (record.Status != KycStatus.Pending)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.InvalidState", "Only pending KYC records can be reviewed."));
        }

        if (targetStatus is not KycStatus.Approved and not KycStatus.Rejected)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.InvalidStatus", "KYC review must transition to approved or rejected."));
        }

        var profile = record.Coach ?? await profileRepository.GetDetailedByUserIdAsync(record.CoachId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<KycRecordDto>(new Error("CoachProfile.NotFound", $"Coach profile {record.CoachId} was not found."));
        }

        record.Status = targetStatus;
        record.AdminNote = adminNote;
        record.ReviewerId = reviewerId;
        record.ReviewedAt = DateTime.UtcNow;

        profile.KycStatus = record.Status;
        profile.KycNote = record.AdminNote;
        profile.UpdatedAt = DateTime.UtcNow;

        var updated = await recordRepository.GetDetailedByIdAsync(record.Id, cancellationToken, asNoTracking: true) ?? record;
        return Result.Success(KycRecordMapping.ToDto(updated));
    }
}
