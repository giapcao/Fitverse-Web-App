using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Timeslots.Queries;

public sealed record ListTimeslotsByCoachQuery(Guid CoachId) : IQuery<IReadOnlyCollection<TimeslotDto>>;
