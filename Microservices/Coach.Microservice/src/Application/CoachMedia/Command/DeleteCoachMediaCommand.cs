using System;
using Application.Abstractions.Messaging;

namespace Application.CoachMedia.Command;

public sealed record DeleteCoachMediaCommand(Guid MediaId) : ICommand;
