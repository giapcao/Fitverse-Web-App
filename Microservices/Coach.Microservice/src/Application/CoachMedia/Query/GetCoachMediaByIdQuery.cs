using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachMedia.Query;

public sealed record GetCoachMediaByIdQuery(Guid MediaId) : IQuery<CoachMediaDto>;
