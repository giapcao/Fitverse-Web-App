using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace Application.AvailabilityRules.Services;

public sealed class AvailabilityRuleScheduler : IAvailabilityRuleScheduler
{
    private const int WeeksToGenerate = 10;

    private readonly ITimeslotRepository _timeslotRepository;
    private readonly ILogger<AvailabilityRuleScheduler> _logger;

    public AvailabilityRuleScheduler(
        ITimeslotRepository timeslotRepository,
        ILogger<AvailabilityRuleScheduler> logger)
    {
        _timeslotRepository = timeslotRepository;
        _logger = logger;
    }

    public async Task RemoveFutureOpenSlotsAsync(AvailabilityRule rule, CancellationToken cancellationToken)
    {
        if (!IsRuleEligible(rule))
        {
            return;
        }

        var timezone = ResolveTimeZone(rule.Timezone);
        var utcNow = DateTime.UtcNow;
        var (rangeStartUtc, rangeEndUtc, _, _) = CalculateWindow(rule, timezone, utcNow);

        var openSlots = await _timeslotRepository
            .GetOpenByCoachAndRangeAsync(rule.CoachId, rangeStartUtc, rangeEndUtc, cancellationToken);

        if (openSlots.Count == 0)
        {
            return;
        }

        var ruleStart = rule.StartTime.ToTimeSpan();
        var ruleEnd = rule.EndTime.ToTimeSpan();
        var targetDay = ConvertToDayOfWeek(rule.Weekday);

        var slotsToDelete = openSlots.Where(slot =>
        {
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(slot.StartAt, timezone);
            if (localStart.DayOfWeek != targetDay)
            {
                return false;
            }

            var startTimeOfDay = localStart.TimeOfDay;
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(slot.EndAt, timezone);
            var endTimeOfDay = localEnd.TimeOfDay;

            return startTimeOfDay >= ruleStart && endTimeOfDay <= ruleEnd;
        }).ToList();

        if (slotsToDelete.Count == 0)
        {
            return;
        }

        _logger.LogDebug(
            "Removing {Count} open timeslots for coach {CoachId} based on availability rule {RuleId}.",
            slotsToDelete.Count,
            rule.CoachId,
            rule.Id);

        _timeslotRepository.DeleteRange(slotsToDelete);
    }

    public async Task EnsureFutureSlotsAsync(AvailabilityRule rule, CancellationToken cancellationToken)
    {
        if (!IsRuleEligible(rule))
        {
            return;
        }

        var timezone = ResolveTimeZone(rule.Timezone);
        var utcNow = DateTime.UtcNow;
        var (rangeStartUtc, rangeEndUtc, localNow, baseDaysUntilTarget) = CalculateWindow(rule, timezone, utcNow);

        var existingSlots = (await _timeslotRepository.FindAsync(
            slot => slot.CoachId == rule.CoachId &&
                    slot.StartAt >= rangeStartUtc &&
                    slot.EndAt <= rangeEndUtc,
            cancellationToken)).ToList();

        var occupiedIntervals = existingSlots
            .Select(slot => (slot.StartAt, slot.EndAt))
            .ToList();

        var newSlots = GenerateSlots(rule, timezone, localNow, baseDaysUntilTarget, occupiedIntervals);
        if (newSlots.Count == 0)
        {
            _logger.LogDebug(
                "No new timeslots generated for coach {CoachId} and availability rule {RuleId}.",
                rule.CoachId,
                rule.Id);
            return;
        }

        await _timeslotRepository.AddRangeAsync(newSlots, cancellationToken);

        _logger.LogInformation(
            "Generated {Count} timeslots for coach {CoachId} from availability rule {RuleId}.",
            newSlots.Count,
            rule.CoachId,
            rule.Id);
    }

    private static bool IsRuleEligible(AvailabilityRule rule)
    {
        return rule.SlotDurationMinutes > 0 && rule.EndTime > rule.StartTime;
    }

    private static (DateTime rangeStartUtc, DateTime rangeEndUtc, DateTime localNow, int baseDaysUntilTarget)
        CalculateWindow(AvailabilityRule rule, TimeZoneInfo timezone, DateTime utcNow)
    {
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timezone);
        var baseDaysUntilTarget = GetBaseDaysUntilTarget(localNow, rule.Weekday, rule.EndTime);

        var lastOccurrenceDate = localNow.Date.AddDays(baseDaysUntilTarget + 7 * (WeeksToGenerate - 1));
        var rangeEndLocal = DateTime.SpecifyKind(
            lastOccurrenceDate.Add(rule.EndTime.ToTimeSpan()),
            DateTimeKind.Unspecified);
        var rangeEndUtc = TimeZoneInfo.ConvertTimeToUtc(rangeEndLocal, timezone);

        return (utcNow, rangeEndUtc, localNow, baseDaysUntilTarget);
    }

    private static List<Timeslot> GenerateSlots(
        AvailabilityRule rule,
        TimeZoneInfo timezone,
        DateTime localNow,
        int baseDaysUntilTarget,
        List<(DateTime StartUtc, DateTime EndUtc)> occupiedIntervals)
    {
        var slots = new List<Timeslot>();
        var slotDuration = TimeSpan.FromMinutes(rule.SlotDurationMinutes);
        var startTime = rule.StartTime.ToTimeSpan();
        var endTime = rule.EndTime.ToTimeSpan();

        var targetDay = ConvertToDayOfWeek(rule.Weekday);
        var localNowUnspecified = DateTime.SpecifyKind(localNow, DateTimeKind.Unspecified);

        for (var week = 0; week < WeeksToGenerate; week++)
        {
            var daysUntilTarget = baseDaysUntilTarget + week * 7;
            var targetDate = localNow.Date.AddDays(daysUntilTarget);
            var dayStart = DateTime.SpecifyKind(targetDate.Add(startTime), DateTimeKind.Unspecified);
            var dayEnd = DateTime.SpecifyKind(targetDate.Add(endTime), DateTimeKind.Unspecified);

            if (dayStart.DayOfWeek != targetDay)
            {
                continue;
            }

            for (var slotStartLocal = dayStart; slotStartLocal < dayEnd; slotStartLocal = slotStartLocal.Add(slotDuration))
            {
                var slotEndLocal = slotStartLocal.Add(slotDuration);
                if (slotEndLocal > dayEnd)
                {
                    break;
                }

                if (slotEndLocal <= localNowUnspecified)
                {
                    continue;
                }

                var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(slotStartLocal, timezone);
                var slotEndUtc = TimeZoneInfo.ConvertTimeToUtc(slotEndLocal, timezone);

                if (HasOverlap(occupiedIntervals, slotStartUtc, slotEndUtc))
                {
                    continue;
                }

                slots.Add(new Timeslot
                {
                    Id = Guid.NewGuid(),
                    CoachId = rule.CoachId,
                    StartAt = slotStartUtc,
                    EndAt = slotEndUtc,
                    Status = SlotStatus.Open,
                    IsOnline = rule.IsOnline,
                    OnsiteLat = rule.IsOnline ? null : rule.OnsiteLat,
                    OnsiteLng = rule.IsOnline ? null : rule.OnsiteLng,
                    Capacity = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                occupiedIntervals.Add((slotStartUtc, slotEndUtc));
            }
        }

        return slots;
    }

    private static bool HasOverlap(
        IEnumerable<(DateTime StartUtc, DateTime EndUtc)> occupiedIntervals,
        DateTime candidateStartUtc,
        DateTime candidateEndUtc)
    {
        foreach (var (startUtc, endUtc) in occupiedIntervals)
        {
            if (candidateStartUtc < endUtc && candidateEndUtc > startUtc)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetBaseDaysUntilTarget(DateTime localNow, int weekday, TimeOnly ruleEndTime)
    {
        var targetDay = ConvertToDayOfWeek(weekday);
        var currentDay = localNow.DayOfWeek;
        var days = ((int)targetDay - (int)currentDay + 7) % 7;

        if (days == 0 && localNow.TimeOfDay >= ruleEndTime.ToTimeSpan())
        {
            days = 7;
        }

        return days;
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static DayOfWeek ConvertToDayOfWeek(int weekday)
    {
        var normalized = ((weekday % 7) + 7) % 7;
        return (DayOfWeek)normalized;
    }
}
