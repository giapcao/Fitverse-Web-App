using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.SubscriptionEvents.Queries;

public sealed record GetSubscriptionEventByIdQuery(Guid Id) : IQuery<SubscriptionEventDto>;
