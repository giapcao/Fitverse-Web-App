using System;

namespace Application.Features;

public record CoachServiceDto(
    Guid Id,
    Guid CoachId,
    Guid SportId,
    string Title,
    string? Description,
    int DurationMinutes,
    int SessionsTotal,
    long PriceVnd,
    bool OnlineAvailable,
    bool OnsiteAvailable,
    string? LocationNote,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
