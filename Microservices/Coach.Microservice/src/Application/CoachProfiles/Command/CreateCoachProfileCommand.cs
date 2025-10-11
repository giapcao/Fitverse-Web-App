using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Command;

public sealed record CreateCoachProfileCommand(
    Guid CoachId,
    string? Bio,
    int? YearsExperience,
    long? BasePriceVnd,
    decimal? ServiceRadiusKm,
    bool IsPublic) : ICommand<CoachProfileDto>;
