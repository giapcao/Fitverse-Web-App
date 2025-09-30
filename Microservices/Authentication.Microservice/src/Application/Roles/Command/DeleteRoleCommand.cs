using Application.Abstractions.Messaging;

namespace Application.Roles.Command;

public sealed record DeleteRoleCommand(string Id) : ICommand;
