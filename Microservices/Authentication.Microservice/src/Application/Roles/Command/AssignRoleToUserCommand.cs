using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Enums;

namespace Application.Roles.Command;

public sealed record AssignRoleToUserCommand(Guid UserId, RoleType Role) : ICommand<UserDto>;

