using System;
using Application.Abstractions.Messaging;
using Application.Features;

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
    int? BookingDurationMinutes) : ICommand<BookingDto>;
