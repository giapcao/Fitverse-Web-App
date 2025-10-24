using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AvailabilityRules.Queries;

public sealed record GetAvailabilityRuleByIdQuery(Guid Id) : IQuery<AvailabilityRuleDto>;
