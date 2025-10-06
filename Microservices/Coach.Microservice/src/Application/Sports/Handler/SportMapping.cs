using Application.Features;
using Domain.Persistence.Models;

namespace Application.Sports.Handler;

internal static class SportMapping
{
    public static SportDto ToDto(Sport sport)
    {
        return new SportDto(sport.Id, sport.DisplayName, sport.Description);
    }
}
