using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.CoachProfiles.Command;

public sealed record UpdateCoachProfileCommand(
    Guid CoachId,
    string? Fullname,
    string? Email,
    string? Bio,
    int? YearsExperience,
    long? BasePriceVnd,
    decimal? ServiceRadiusKm,
    DateOnly? BirthDate,
    decimal? WeightKg,
    decimal? HeightCm,
    string? Gender,
    string? OperatingLocation,
    string? TaxCode,
    string? CitizenId,
    DateOnly? CitizenIssueDate,
    string? CitizenIssuePlace,
    bool? IsPublic,
    KycStatus? KycStatus) : ICommand<CoachProfileDto>;
