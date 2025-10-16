using System;

namespace Application.Features;

public record AvailabilityRuleDto(
    Guid Id,
    Guid CoachId,
    int Weekday,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    bool IsOnline,
    double? OnsiteLat,
    double? OnsiteLng,
    string Timezone,
    DateTime CreatedAt,
    DateTime UpdatedAt);
