using Application.Features;
using Domain.Persistence.Models;

namespace Application.CoachCertifications.Handler;

internal static class CoachCertificationMapping
{
    public static CoachCertificationDto ToDto(CoachCertification certification)
    {
        return new CoachCertificationDto(
            certification.Id,
            certification.CoachId,
            certification.CertName,
            certification.Issuer,
            certification.IssuedOn,
            certification.ExpiresOn,
            certification.FileUrl,
            certification.FileUrl,
            certification.Status,
            certification.ReviewedBy,
            certification.ReviewedAt,
            certification.CreatedAt,
            certification.UpdatedAt);
    }
}
