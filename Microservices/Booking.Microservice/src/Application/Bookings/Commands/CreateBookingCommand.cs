using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    Guid UserId,
    Guid CoachId,
    Guid? TimeslotId,
    DateTime StartAt,
    DateTime EndAt,
    BookingStatus Status = BookingStatus.PendingPayment,
    string? LocationNote = null,
    string? Notes = null,
    int? DurationMinutes = null) : ICommand<BookingDto>;
