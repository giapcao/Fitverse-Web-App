using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachCertifications.Command;

public sealed record UpdateCoachCertificationCommand(
    Guid CertificationId,
    string? CertName,
    string? Issuer,
    DateOnly? IssuedOn,
    DateOnly? ExpiresOn,
    string? FileUrl,
    string? Directory = "certifications",
    CoachCertificationFile? File = null) : ICommand<CoachCertificationDto>;
