using Application.Features;
using Domain.Persistence.Models;

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
            record.ReviewerId);
    }
}
