using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachCertifications.Query;

public sealed record ListCoachCertificationsQuery(Guid? CoachId) : IQuery<IEnumerable<CoachCertificationDto>>;
