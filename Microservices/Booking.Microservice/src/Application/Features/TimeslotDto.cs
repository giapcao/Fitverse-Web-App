using System;
using System.Collections.Generic;

namespace Application.Features;

public record TimeslotDto(
    Guid Id,
    Guid CoachId,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    bool IsOnline,
    double? OnsiteLat,
    double? OnsiteLng,
    int Capacity,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<BookingSummaryDto> Bookings,
    IReadOnlyCollection<SubscriptionEventSummaryDto> SubscriptionEvents);

public record BookingSummaryDto(
    Guid Id,
    Guid UserId,
    Guid CoachId,
    Guid ServiceId,
    DateTime StartAt,
    DateTime EndAt,
    string Status);
