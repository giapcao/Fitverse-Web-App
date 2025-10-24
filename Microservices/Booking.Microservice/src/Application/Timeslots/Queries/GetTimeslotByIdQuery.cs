using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Timeslots.Queries;

public sealed record GetTimeslotByIdQuery(Guid Id) : IQuery<TimeslotDto>;
