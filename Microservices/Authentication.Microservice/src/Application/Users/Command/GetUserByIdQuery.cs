using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Users.Command;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;
