using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Query;

public sealed record ListCoachProfilesQuery(int PageNumber = 1, int PageSize = 10)
    : PagedQuery<CoachProfileDto>(PageNumber, PageSize);
