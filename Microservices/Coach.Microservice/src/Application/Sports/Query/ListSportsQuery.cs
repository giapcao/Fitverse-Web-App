using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Sports.Query;

public sealed record ListSportsQuery(int PageNumber = 1, int PageSize = 10)
    : PagedQuery<SportDto>(PageNumber, PageSize);
