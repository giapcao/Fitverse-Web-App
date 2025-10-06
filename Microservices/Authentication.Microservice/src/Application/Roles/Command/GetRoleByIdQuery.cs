using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Roles.Command;

public sealed record GetRoleByIdQuery(Guid Id) : IQuery<RoleDto>;
