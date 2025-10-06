using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachServices.Query;

public sealed record GetCoachServiceByIdQuery(Guid ServiceId) : IQuery<CoachServiceDto>;
