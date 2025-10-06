using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Sports.Query;

public sealed record GetSportByIdQuery(Guid SportId) : IQuery<SportDto>;
