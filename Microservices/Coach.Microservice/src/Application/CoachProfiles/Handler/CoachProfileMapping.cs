using System;
using System.Linq;
using Application.Features;
using Domain.Persistence.Models;

namespace Application.CoachProfiles.Handler;

internal static class CoachProfileMapping
{
    public static CoachProfileDto ToDto(CoachProfile profile)
    {
        var media = profile.CoachMedia?.Select(CoachMediaMapping.ToDto).ToArray() ?? Array.Empty<CoachMediaDto>();
        var services = profile.CoachServices?.Select(CoachServiceMapping.ToDto).ToArray() ?? Array.Empty<CoachServiceDto>();
        var kycRecords = profile.KycRecords?.Select(KycRecordMapping.ToDto).ToArray() ?? Array.Empty<KycRecordDto>();
        var sportIds = profile.Sports?.Select(s => s.Id).ToArray() ?? Array.Empty<Guid>();

        return new CoachProfileDto(
            profile.UserId,
            profile.Bio,
            profile.YearsExperience,
            profile.BasePriceVnd,
            profile.ServiceRadiusKm,
            profile.KycNote,
            profile.KycStatus,
            profile.RatingAvg,
            profile.RatingCount,
            profile.IsPublic,
            profile.CreatedAt,
            profile.UpdatedAt,
            sportIds,
            media,
            services,
            kycRecords);
    }
}

internal static class CoachMediaMapping
{
    public static CoachMediaDto ToDto(CoachMedium medium)
    {
        return new CoachMediaDto(
            medium.Id,
            medium.CoachId,
            medium.MediaName,
            medium.MediaType,
            medium.Url,
            medium.Status,
            medium.IsFeatured,
            medium.CreatedAt,
            medium.UpdatedAt);
    }
}

internal static class CoachServiceMapping
{
    public static CoachServiceDto ToDto(CoachService service)
    {
        return new CoachServiceDto(
            service.Id,
            service.CoachId,
            service.SportId,
            service.Title,
            service.Description,
            service.DurationMinutes,
            service.SessionsTotal,
            service.PriceVnd,
            service.OnlineAvailable,
            service.OnsiteAvailable,
            service.LocationNote,
            service.IsActive,
            service.CreatedAt,
            service.UpdatedAt);
    }
}

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
