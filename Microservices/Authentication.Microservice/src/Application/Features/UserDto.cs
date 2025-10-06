using System;
using System.Collections.Generic;

namespace Application.Features;

public sealed record UserDto(
    Guid Id,
    string? Email,
    string? Phone,
    string FullName,
    string? AvatarUrl,
    string? Gender,
    DateOnly? Birth,
    string? Description,
    double? HomeLat,
    double? HomeLng,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<Guid> Roles);

