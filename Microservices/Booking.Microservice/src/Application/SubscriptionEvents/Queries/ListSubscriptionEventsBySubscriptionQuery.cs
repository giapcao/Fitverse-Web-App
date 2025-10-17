using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.SubscriptionEvents.Queries;

public sealed record ListSubscriptionEventsBySubscriptionQuery(Guid SubscriptionId) : IQuery<IReadOnlyCollection<SubscriptionEventDto>>;
