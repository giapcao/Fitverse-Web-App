using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Query;

public sealed record ListCoachProfilesQuery : IQuery<IEnumerable<CoachProfileDto>>;
