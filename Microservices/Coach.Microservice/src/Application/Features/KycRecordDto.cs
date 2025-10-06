using System;
using Domain.Persistence.Enums;

namespace Application.Features;

public record KycRecordDto(
    Guid Id,
    Guid CoachId,
    string? IdDocumentUrl,
    string? AdminNote,
    KycStatus Status,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    Guid? ReviewerId);
