using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachCertifications.Query;

public sealed record ListAllCoachCertificationsQuery(int PageNumber = 1, int PageSize = 10)
    : PagedQuery<CoachCertificationDto>(PageNumber, PageSize);
