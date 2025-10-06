using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachServices.Query;

public sealed record ListCoachServicesQuery(Guid? CoachId, Guid? SportId) : IQuery<IEnumerable<CoachServiceDto>>;
