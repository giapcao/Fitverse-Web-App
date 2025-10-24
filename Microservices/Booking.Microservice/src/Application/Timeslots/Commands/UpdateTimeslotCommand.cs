using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.Timeslots.Commands;

public sealed record UpdateTimeslotCommand(
    Guid Id,
    Guid CoachId,
    DateTime StartAt,
    DateTime EndAt,
    SlotStatus Status,
    bool IsOnline,
    double? OnsiteLat,
    double? OnsiteLng,
    int Capacity) : ICommand<TimeslotDto>;
