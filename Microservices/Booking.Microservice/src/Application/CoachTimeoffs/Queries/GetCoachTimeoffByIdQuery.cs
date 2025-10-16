using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachTimeoffs.Queries;

public sealed record GetCoachTimeoffByIdQuery(Guid Id) : IQuery<CoachTimeoffDto>;
