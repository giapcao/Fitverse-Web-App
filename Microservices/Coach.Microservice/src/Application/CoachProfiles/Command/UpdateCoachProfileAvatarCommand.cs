using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Command;

public sealed record UpdateCoachProfileAvatarCommand(
    Guid CoachId,
    string? Directory = null,
    CoachAvatarFile? File = null) : ICommand<CoachProfileDto>;
