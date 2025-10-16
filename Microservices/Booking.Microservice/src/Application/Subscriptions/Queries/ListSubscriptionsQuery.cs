using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Subscriptions.Queries;

public sealed record ListSubscriptionsQuery : IQuery<IReadOnlyCollection<SubscriptionDto>>;
