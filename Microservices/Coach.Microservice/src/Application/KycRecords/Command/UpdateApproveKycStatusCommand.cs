using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.KycRecords.Command;

public sealed record UpdateApproveKycStatusCommand(
    Guid RecordId,
    string? AdminNote,
    Guid? ReviewerId) : ICommand<KycRecordDto>;
