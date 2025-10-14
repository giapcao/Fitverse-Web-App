using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachMedia.Query;

public sealed record ListCoachMediaQuery(Guid? CoachId, bool? IsFeatured = null, int PageNumber = 1, int PageSize = 10)
    : PagedQuery<CoachMediaDto>(PageNumber, PageSize);
