using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.CoachMedia.Command;

public sealed record UpdateCoachMediaCommand(
    Guid MediaId,
    string? MediaName,
    string? Description,
    CoachMediaType? MediaType,
    string? Url,
    bool? IsFeatured,
    string? Directory = null,
    CoachMediaFile? File = null) : ICommand<CoachMediaDto>;
