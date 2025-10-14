using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachCertifications.Command;

public sealed record InactivateCoachCertificationCommand(Guid CertificationId) : ICommand<CoachCertificationDto>;
