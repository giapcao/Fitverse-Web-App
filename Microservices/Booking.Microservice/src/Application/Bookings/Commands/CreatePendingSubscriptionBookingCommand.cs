using System;
using Application.Abstractions.Messaging;
using Application.Features;
using SharedLibrary.Contracts.Payments;

namespace Application.Bookings.Commands;

public sealed record CreatePendingSubscriptionBookingCommand(
    Guid TimeslotId,
    Guid UserId,
    Guid ServiceId,
    int SubscriptionSessionsTotal,
    decimal SubscriptionCommissionPct,
    long SubscriptionNetAmountVnd,
    string? BookingLocationNote,
    string? BookingNotes,
    int? BookingDurationMinutes,
    Guid? CorrelationId,
    Gateway Gateway,
    PaymentFlow Flow,
    Guid? WalletId,
    string? ClientIp) : ICommand<BookingDto>;
