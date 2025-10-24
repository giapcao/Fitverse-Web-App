using System;
using System.Collections.Generic;

namespace Application.Features;

public record BookingDto(
    Guid Id,
    Guid UserId,
    Guid CoachId,
    Guid? TimeslotId,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    long GrossAmountVnd,
    decimal CommissionPct,
    long CommissionVnd,
    long NetAmountVnd,
    string CurrencyCode,
    string? LocationNote,
    string? Notes,
    int? DurationMinutes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    TimeslotSummaryDto? Timeslot,
    IReadOnlyCollection<SubscriptionEventSummaryDto> SubscriptionEvents);

public record TimeslotSummaryDto(
    Guid Id,
    DateTime StartAt,
    DateTime EndAt,
    string Status);

public record SubscriptionEventSummaryDto(
    Guid Id,
    string EventType,
    DateTime CreatedAt,
    Guid? BookingId,
    Guid? TimeslotId);
