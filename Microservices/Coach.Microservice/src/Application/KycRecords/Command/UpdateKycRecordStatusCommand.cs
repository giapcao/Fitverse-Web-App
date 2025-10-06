using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.KycRecords.Command;

public sealed record UpdateKycRecordStatusCommand(
    Guid RecordId,
    KycStatus Status,
    string? AdminNote,
    Guid? ReviewerId) : ICommand<KycRecordDto>;
