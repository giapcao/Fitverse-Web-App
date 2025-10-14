using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachCertifications.Command;

public sealed record CreateCoachCertificationCommand(
    Guid CoachId,
    string CertName,
    string? Issuer,
    DateOnly? IssuedOn,
    DateOnly? ExpiresOn,
    string? FileUrl,
    string? Directory = null,
    CoachCertificationFile? File = null) : ICommand<CoachCertificationDto>;
