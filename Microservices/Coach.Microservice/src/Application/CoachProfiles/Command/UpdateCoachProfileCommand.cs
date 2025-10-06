using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.CoachProfiles.Command;

public sealed record UpdateCoachProfileCommand(
    Guid CoachId,
    string? Bio,
    int? YearsExperience,
    long? BasePriceVnd,
    decimal? ServiceRadiusKm,
    string? KycNote,
    bool? IsPublic,
    KycStatus? KycStatus) : ICommand<CoachProfileDto>;
