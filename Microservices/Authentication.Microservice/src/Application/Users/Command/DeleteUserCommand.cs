using System;
using Application.Abstractions.Messaging;

namespace Application.Users.Command;

public sealed record DeleteUserCommand(Guid Id) : ICommand;
