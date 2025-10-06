using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Application.Features;

public record CoachProfileDto(
    Guid CoachId,
    string? Bio,
    int? YearsExperience,
    long? BasePriceVnd,
    decimal? ServiceRadiusKm,
    string? KycNote,
    KycStatus KycStatus,
    decimal? RatingAvg,
    int RatingCount,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<Guid> SportIds,
    IReadOnlyCollection<CoachMediaDto> Media,
    IReadOnlyCollection<CoachServiceDto> Services,
    IReadOnlyCollection<KycRecordDto> KycRecords);
