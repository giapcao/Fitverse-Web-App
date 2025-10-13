using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Command;

public sealed record CreateCoachProfileCommand(
    Guid CoachId,
    string? Fullname,
    string? Bio,
    int? YearsExperience,
    long? BasePriceVnd,
    decimal? ServiceRadiusKm,
    string? AvatarUrl,
    DateOnly? BirthDate,
    decimal? WeightKg,
    decimal? HeightCm,
    string? Gender,
    string? OperatingLocation,
    string? TaxCode,
    string? CitizenId,
    DateOnly? CitizenIssueDate,
    string? CitizenIssuePlace,
    bool IsPublic) : ICommand<CoachProfileDto>;
