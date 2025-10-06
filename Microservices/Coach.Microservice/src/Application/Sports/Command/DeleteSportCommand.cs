using System;
using Application.Abstractions.Messaging;

namespace Application.Sports.Command;

public sealed record DeleteSportCommand(Guid SportId) : ICommand;
