using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Users.Command;

public sealed record UpdateUserAvatarCommand(
    Guid UserId,
    string? Directory = null,
    UserAvatarFile? File = null) : ICommand<UserDto>;
