using System;
using Domain.Persistence.Enums;

namespace Application.Features;

public record CoachMediaDto(
    Guid Id,
    Guid CoachId,
    string? MediaName,
    string? Description,
    CoachMediaType MediaType,
    string Url,
    string DownloadUrl,
    bool Status,
    bool IsFeatured,
    DateTime CreatedAt,
    DateTime UpdatedAt);
