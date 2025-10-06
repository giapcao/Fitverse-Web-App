using System;
using Application.Abstractions.Messaging;

namespace Application.CoachServices.Command;

public sealed record DeleteCoachServiceCommand(Guid ServiceId) : ICommand;
