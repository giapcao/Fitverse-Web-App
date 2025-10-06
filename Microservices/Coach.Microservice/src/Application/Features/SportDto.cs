using System;

namespace Application.Features;

public record SportDto(
    Guid Id,
    string DisplayName,
    string? Description);
