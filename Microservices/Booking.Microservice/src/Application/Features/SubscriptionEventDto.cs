using System;

namespace Application.Features;

public record SubscriptionEventDto(
    Guid Id,
    Guid SubscriptionId,
    string EventType,
    Guid? BookingId,
    Guid? TimeslotId,
    DateTime CreatedAt);
