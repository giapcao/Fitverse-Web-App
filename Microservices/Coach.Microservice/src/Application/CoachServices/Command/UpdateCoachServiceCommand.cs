using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachServices.Command;

public sealed record UpdateCoachServiceCommand(
    Guid ServiceId,
    Guid? SportId,
    string? Title,
    string? Description,
    int? DurationMinutes,
    int? SessionsTotal,
    long? PriceVnd,
    bool? OnlineAvailable,
    bool? OnsiteAvailable,
    string? LocationNote,
    bool? IsActive) : ICommand<CoachServiceDto>;
