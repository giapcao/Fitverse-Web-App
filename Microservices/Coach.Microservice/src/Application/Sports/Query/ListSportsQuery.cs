using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Sports.Query;

public sealed record ListSportsQuery : IQuery<IEnumerable<SportDto>>;
