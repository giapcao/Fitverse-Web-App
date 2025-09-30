using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Roles.Command;

public sealed record ListRolesQuery : IQuery<IEnumerable<RoleDto>>;
