using System;

namespace Application.Features;

public record CoachCertificationDto(
    Guid Id,
    Guid CoachId,
    string CertName,
    string? Issuer,
    DateOnly? IssuedOn,
    DateOnly? ExpiresOn,
    string? FileUrl,
    string? FileDownloadUrl,
    string Status,
    Guid? ReviewedBy,
    DateTime? ReviewedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
