using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.CoachMedia.Command;

public sealed record CreateCoachMediaCommand(
    Guid CoachId,
    string? MediaName,
    CoachMediaType MediaType,
    string Url,
    bool Status,
    bool IsFeatured) : ICommand<CoachMediaDto>;
