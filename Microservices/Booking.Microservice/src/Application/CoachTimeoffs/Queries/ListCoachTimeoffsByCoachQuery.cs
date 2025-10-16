using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachTimeoffs.Queries;

public sealed record ListCoachTimeoffsByCoachQuery(Guid CoachId) : IQuery<IReadOnlyCollection<CoachTimeoffDto>>;
