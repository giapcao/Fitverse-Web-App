using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Users.Command;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FullName,
    string? Phone,
    string? Gender,
    DateOnly? Birth,
    string? Description,
    double? HomeLat,
    double? HomeLng,
    bool? IsActive,
    bool? EmailConfirmed,
    IEnumerable<Guid>? RoleIds) : ICommand<UserDto>;

