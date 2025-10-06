using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.KycRecords.Query;

public sealed record GetKycRecordByIdQuery(Guid RecordId) : IQuery<KycRecordDto>;
