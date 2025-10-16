using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AvailabilityRules.Queries;

public sealed record ListAvailabilityRulesQuery : IQuery<IReadOnlyCollection<AvailabilityRuleDto>>;
