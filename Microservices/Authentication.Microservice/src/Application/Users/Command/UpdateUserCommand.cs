using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Users.Command;

public sealed record UpdateUserCommand(
    Guid Id,
    string? Email,
    string? FullName,
    string? Phone,
    string? AvatarUrl,
    string? Gender,
    DateOnly? Birth,
    string? Description,
    double? HomeLat,
    double? HomeLng
    ) : ICommand<UserDto>;
