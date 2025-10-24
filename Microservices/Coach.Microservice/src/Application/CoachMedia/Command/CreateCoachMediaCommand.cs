using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.CoachMedia.Command;

public sealed record CreateCoachMediaCommand(
    Guid CoachId,
    string? MediaName,
    string? Description,
    CoachMediaType MediaType,
    string? Url,
    bool IsFeatured,
    string? Directory = "media",
    CoachMediaFile? File = null) : ICommand<CoachMediaDto>;
