using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.KycRecords.Query;

public sealed record ListKycRecordsQuery(Guid? CoachId) : IQuery<IEnumerable<KycRecordDto>>;
