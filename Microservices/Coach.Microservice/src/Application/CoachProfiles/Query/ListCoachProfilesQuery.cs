using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Query;

public sealed record ListCoachProfilesQuery(
    string? OperatingLocation = null,
    decimal? MinPriceVnd = null,
    decimal? MaxPriceVnd = null,
    decimal? MinRating = null,
    string? Gender = null,
    int PageNumber = 1,
    int PageSize = 10)
    : PagedQuery<CoachProfileDto>(PageNumber, PageSize);
