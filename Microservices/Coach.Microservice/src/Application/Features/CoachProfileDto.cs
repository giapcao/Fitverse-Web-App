using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Application.Features;

public record CoachProfileDto(
    Guid CoachId,
    string? Fullname,
    string? Email,
    string? Bio,
    int? YearsExperience,
    long? BasePriceVnd,
    decimal? ServiceRadiusKm,
    string? AvatarUrl,
    string? AvatarDownloadUrl,
    DateOnly? BirthDate,
    decimal? WeightKg,
    decimal? HeightCm,
    string? Gender,
    string? OperatingLocation,
    string? TaxCode,
    string? CitizenId,
    DateOnly? CitizenIssueDate,
    string? CitizenIssuePlace,
    string? KycNote,
    KycStatus KycStatus,
    decimal? RatingAvg,
    int RatingCount,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<CoachProfileSportDto> Sports,
    IReadOnlyCollection<CoachMediaDto> Media,
    IReadOnlyCollection<CoachServiceDto> Services,
    IReadOnlyCollection<CoachCertificationDto> Certifications);
