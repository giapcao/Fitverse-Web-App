using System;
using System.Collections.Generic;

namespace Application.Features;

public record SubscriptionDto(
    Guid Id,
    Guid UserId,
    Guid CoachId,
    Guid ServiceId,
    string Status,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int SessionsTotal,
    int SessionsReserved,
    int SessionsConsumed,
    long PriceGrossVnd,
    decimal CommissionPct,
    long CommissionVnd,
    long NetAmountVnd,
    string CurrencyCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<SubscriptionEventDto> Events);
