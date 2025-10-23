using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.Bookings.Commands;

public sealed record UpdateBookingCommand(
    Guid Id,
    Guid UserId,
    Guid CoachId,
    Guid? TimeslotId,
    DateTime StartAt,
    DateTime EndAt,
    BookingStatus Status,
    long GrossAmountVnd,
    decimal CommissionPct,
    long CommissionVnd,
    long NetAmountVnd,
    string CurrencyCode,
    string? LocationNote,
    string? Notes,
    int? DurationMinutes) : ICommand<BookingDto>;
