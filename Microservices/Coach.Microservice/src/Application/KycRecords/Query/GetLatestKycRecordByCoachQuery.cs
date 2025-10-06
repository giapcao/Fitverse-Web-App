using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.KycRecords.Query;

public sealed record GetLatestKycRecordByCoachQuery(Guid CoachId) : IQuery<KycRecordDto>;
