using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Timeslots.Queries;

public sealed record ListOpenTimeslotsByCoachQuery(Guid CoachId, DateTime From, DateTime To) : IQuery<IReadOnlyCollection<TimeslotDto>>;
