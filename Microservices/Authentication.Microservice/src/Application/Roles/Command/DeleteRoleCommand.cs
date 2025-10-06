using System;
using Application.Abstractions.Messaging;

namespace Application.Roles.Command;

public sealed record DeleteRoleCommand(Guid Id) : ICommand;
