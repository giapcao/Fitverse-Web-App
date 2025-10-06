using System;
using Domain.Persistence.Enums;

namespace Application.Features;

public record CoachMediaDto(
    Guid Id,
    Guid CoachId,
    string? MediaName,
    CoachMediaType MediaType,
    string Url,
    bool Status,
    bool IsFeatured,
    DateTime CreatedAt,
    DateTime UpdatedAt);
