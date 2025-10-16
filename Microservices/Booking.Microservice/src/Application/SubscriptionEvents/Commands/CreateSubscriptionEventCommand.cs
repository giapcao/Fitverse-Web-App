using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.SubscriptionEvents.Commands;

public sealed record CreateSubscriptionEventCommand(
    Guid SubscriptionId,
    SubscriptionEventType EventType,
    Guid? BookingId,
    Guid? TimeslotId) : ICommand<SubscriptionEventDto>;
