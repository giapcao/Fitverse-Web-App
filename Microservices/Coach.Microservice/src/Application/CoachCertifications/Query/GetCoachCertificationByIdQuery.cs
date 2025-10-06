using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachCertifications.Query;

public sealed record GetCoachCertificationByIdQuery(Guid CertificationId) : IQuery<CoachCertificationDto>;
