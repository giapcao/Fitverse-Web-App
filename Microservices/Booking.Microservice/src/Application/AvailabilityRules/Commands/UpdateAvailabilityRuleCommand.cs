using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AvailabilityRules.Commands;

public sealed record UpdateAvailabilityRuleCommand(
    Guid Id,
    Guid CoachId,
    int Weekday,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    bool IsOnline,
    double? OnsiteLat,
    double? OnsiteLng,
    string Timezone) : ICommand<AvailabilityRuleDto>;
