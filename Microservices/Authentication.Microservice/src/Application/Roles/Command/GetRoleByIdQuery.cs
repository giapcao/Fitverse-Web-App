using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Roles.Command;

public sealed record GetRoleByIdQuery(string Id) : IQuery<RoleDto>;
