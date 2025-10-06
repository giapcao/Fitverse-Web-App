using System;
using Application.Abstractions.Messaging;

namespace Application.CoachProfiles.Command;

public sealed record DeleteCoachProfileCommand(Guid CoachId) : ICommand;
