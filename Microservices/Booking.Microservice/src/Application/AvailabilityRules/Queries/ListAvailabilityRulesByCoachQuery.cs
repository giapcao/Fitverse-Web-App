using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AvailabilityRules.Queries;

public sealed record ListAvailabilityRulesByCoachQuery(Guid CoachId) : IQuery<IReadOnlyCollection<AvailabilityRuleDto>>;
