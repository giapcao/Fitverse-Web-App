using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Roles.Command;

public sealed record UpdateRoleCommand(string Id, string DisplayName) : ICommand<RoleDto>;
