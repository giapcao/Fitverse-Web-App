using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachMedia.Query;

public sealed record ListCoachMediaQuery(Guid? CoachId) : IQuery<IEnumerable<CoachMediaDto>>;
