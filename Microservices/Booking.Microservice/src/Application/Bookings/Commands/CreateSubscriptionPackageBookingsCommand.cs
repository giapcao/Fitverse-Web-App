using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Bookings.Commands;

public sealed record CreateSubscriptionPackageBookingsCommand(
    Guid UserId,
    Guid CoachId,
    IReadOnlyCollection<Guid> TimeslotIds,
    string? LocationNote,
    string? Notes) : ICommand<IReadOnlyCollection<BookingDto>>;
