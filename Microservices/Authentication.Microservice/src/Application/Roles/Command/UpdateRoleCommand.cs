using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Roles.Command;

public sealed record UpdateRoleCommand(Guid Id, string DisplayName) : ICommand<RoleDto>;
