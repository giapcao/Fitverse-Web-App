using System;

namespace Application.Features;

public record CoachProfileSummaryDto(
    Guid CoachId,
    string? Fullname,
    string? Email,
    string? AvatarUrl,
    string? AvatarDownloadUrl,
    string? OperatingLocation,
    long? BasePriceVnd,
    decimal? RatingAvg,
    int RatingCount,
    bool IsPublic);
