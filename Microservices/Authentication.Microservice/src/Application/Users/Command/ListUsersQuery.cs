using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Users.Command;

public sealed record ListUsersQuery : IQuery<IEnumerable<UserDto>>;
