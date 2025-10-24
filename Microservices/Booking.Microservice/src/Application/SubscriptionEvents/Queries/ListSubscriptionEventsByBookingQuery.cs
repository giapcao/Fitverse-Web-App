using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.SubscriptionEvents.Queries;

public sealed record ListSubscriptionEventsByBookingQuery(Guid BookingId) : IQuery<IReadOnlyCollection<SubscriptionEventDto>>;
