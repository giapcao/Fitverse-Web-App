using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    Guid UserId,
    Guid CoachId,
    Guid ServiceId,
    Guid? TimeslotId,
    DateTime StartAt,
    DateTime EndAt,
    BookingStatus Status = BookingStatus.PendingPayment,
    long GrossAmountVnd = 0,
    decimal CommissionPct = 15.00m,
    long CommissionVnd = 0,
    long NetAmountVnd = 0,
    string CurrencyCode = "VND",
    string? LocationNote = null,
    string? Notes = null,
    string? ServiceTitle = null,
    int? DurationMinutes = null) : ICommand<BookingDto>;
