using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Roles.Command;

public sealed record CreateRoleCommand(string Id, string DisplayName) : ICommand<RoleDto>;
