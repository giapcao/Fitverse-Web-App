using System;
using System.Linq;
using CoachCertificationMapper = Application.CoachCertifications.Handler.CoachCertificationMapping;
using Application.Features;
using Domain.Persistence.Models;

namespace Application.CoachProfiles.Handler;

internal static class CoachProfileMapping
{
    public static CoachProfileSummaryDto ToSummaryDto(CoachProfile profile)
    {
        return new CoachProfileSummaryDto(
            profile.UserId,
            profile.Fullname,
            profile.AvatarUrl,
            profile.AvatarUrl,
            profile.OperatingLocation,
            profile.BasePriceVnd,
            profile.RatingAvg,
            profile.RatingCount,
            profile.IsPublic);
    }

    public static CoachProfileDto ToDto(CoachProfile profile)
    {
        var media = profile.CoachMedia?.Select(CoachMediaMapping.ToDto).ToArray() ?? Array.Empty<CoachMediaDto>();
        var services = profile.CoachServices?.Select(CoachServiceMapping.ToDto).ToArray() ?? Array.Empty<CoachServiceDto>();
        var certifications = profile.CoachCertifications?
            .Select(CoachCertificationMapper.ToDto)
            .ToArray() ?? Array.Empty<CoachCertificationDto>();
        var sports = profile.Sports?
            .Select(s => new CoachProfileSportDto(s.Id, s.DisplayName))
            .ToArray() ?? Array.Empty<CoachProfileSportDto>();

        return new CoachProfileDto(
            profile.UserId,
            profile.Fullname,
            profile.Bio,
            profile.YearsExperience,
            profile.BasePriceVnd,
            profile.ServiceRadiusKm,
            profile.AvatarUrl,
            profile.AvatarUrl,
            profile.BirthDate,
            profile.WeightKg,
            profile.HeightCm,
            profile.Gender,
            profile.OperatingLocation,
            profile.TaxCode,
            profile.CitizenId,
            profile.CitizenIssueDate,
            profile.CitizenIssuePlace,
            profile.KycNote,
            profile.KycStatus,
            profile.RatingAvg,
            profile.RatingCount,
            profile.IsPublic,
            profile.CreatedAt,
            profile.UpdatedAt,
            sports,
            media,
            services,
            certifications);
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
            medium.Description,
            medium.MediaType,
            medium.Url,
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
