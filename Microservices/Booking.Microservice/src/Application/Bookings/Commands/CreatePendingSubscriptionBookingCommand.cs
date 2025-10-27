using System;
using Application.Abstractions.Messaging;
using Application.Features;
using SharedLibrary.Contracts.Payments;

namespace Application.Bookings.Commands;

public sealed record CreatePendingSubscriptionBookingCommand(
    Guid TimeslotId,
    Guid UserId,
    Guid ServiceId,
    DateTime SubscriptionPeriodStart,
    DateTime SubscriptionPeriodEnd,
    int SubscriptionSessionsTotal,
    long SubscriptionPriceGrossVnd,
    decimal SubscriptionCommissionPct,
    long SubscriptionCommissionVnd,
    long SubscriptionNetAmountVnd,
    string SubscriptionCurrencyCode,
    long BookingGrossAmountVnd,
    decimal BookingCommissionPct,
    long BookingCommissionVnd,
    long BookingNetAmountVnd,
    string BookingCurrencyCode,
    string? BookingLocationNote,
    string? BookingNotes,
    int? BookingDurationMinutes,
    Guid? CorrelationId,
    Gateway Gateway,
    PaymentFlow Flow,
    Guid? WalletId,
    string? ClientIp) : ICommand<BookingDto>;
