using Application.Features;
using Domain.Persistence.Models;

namespace Application.CoachServices.Handler;

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
