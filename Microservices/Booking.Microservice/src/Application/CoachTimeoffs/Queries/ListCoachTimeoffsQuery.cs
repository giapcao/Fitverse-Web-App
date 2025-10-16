using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachTimeoffs.Queries;

public sealed record ListCoachTimeoffsQuery : IQuery<IReadOnlyCollection<CoachTimeoffDto>>;
