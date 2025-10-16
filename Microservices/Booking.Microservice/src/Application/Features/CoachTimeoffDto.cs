using System;

namespace Application.Features;

public record CoachTimeoffDto(
    Guid Id,
    Guid CoachId,
    DateTime StartAt,
    DateTime EndAt,
    string? Reason,
    DateTime CreatedAt);
