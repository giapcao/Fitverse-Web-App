using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachServices.Query;

public sealed record ListAllCoachServicesQuery(int PageNumber = 1, int PageSize = 10)
    : PagedQuery<CoachServiceDto>(PageNumber, PageSize);
