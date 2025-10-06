using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.KycRecords.Command;

public sealed record CreateKycRecordCommand(
    Guid CoachId,
    string? IdDocumentUrl,
    string? AdminNote) : ICommand<KycRecordDto>;
