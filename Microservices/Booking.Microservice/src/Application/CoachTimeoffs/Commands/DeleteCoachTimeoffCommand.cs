using System;
using Application.Abstractions.Messaging;

namespace Application.CoachTimeoffs.Commands;

public sealed record DeleteCoachTimeoffCommand(Guid Id) : ICommand;
