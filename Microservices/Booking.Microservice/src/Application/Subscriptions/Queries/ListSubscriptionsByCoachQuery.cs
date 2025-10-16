using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Subscriptions.Queries;

public sealed record ListSubscriptionsByCoachQuery(Guid CoachId) : IQuery<IReadOnlyCollection<SubscriptionDto>>;
