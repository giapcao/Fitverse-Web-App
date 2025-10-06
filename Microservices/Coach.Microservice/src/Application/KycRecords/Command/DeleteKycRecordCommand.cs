using System;
using Application.Abstractions.Messaging;

namespace Application.KycRecords.Command;

public sealed record DeleteKycRecordCommand(Guid RecordId) : ICommand;
