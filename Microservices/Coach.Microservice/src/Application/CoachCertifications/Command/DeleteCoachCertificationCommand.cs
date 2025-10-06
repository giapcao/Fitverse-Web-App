using System;
using Application.Abstractions.Messaging;

namespace Application.CoachCertifications.Command;

public sealed record DeleteCoachCertificationCommand(Guid CertificationId) : ICommand;
