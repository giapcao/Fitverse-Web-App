using Application.Features;
using Domain.Persistence.Models;

namespace Application.CoachMedia.Handler;

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
