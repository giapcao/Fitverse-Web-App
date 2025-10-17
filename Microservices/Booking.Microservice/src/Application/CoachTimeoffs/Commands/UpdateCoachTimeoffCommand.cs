using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachTimeoffs.Commands;

public sealed record UpdateCoachTimeoffCommand(
    Guid Id,
    Guid CoachId,
    DateTime StartAt,
    DateTime EndAt,
    string? Reason) : ICommand<CoachTimeoffDto>;
