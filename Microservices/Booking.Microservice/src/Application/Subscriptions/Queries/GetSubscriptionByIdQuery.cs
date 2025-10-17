using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Subscriptions.Queries;

public sealed record GetSubscriptionByIdQuery(Guid Id) : IQuery<SubscriptionDto>;
