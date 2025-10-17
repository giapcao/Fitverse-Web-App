using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.Subscriptions.Commands;

public sealed record UpdateSubscriptionCommand(
    Guid Id,
    Guid UserId,
    Guid CoachId,
    Guid ServiceId,
    SubscriptionStatus Status,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int SessionsTotal,
    int SessionsReserved,
    int SessionsConsumed,
    long PriceGrossVnd,
    decimal CommissionPct,
    long CommissionVnd,
    long NetAmountVnd,
    string CurrencyCode) : ICommand<SubscriptionDto>;
