using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Bookings.Commands;

public sealed record CancelPendingSubscriptionBookingCommand(
    Guid SubscriptionId,
    Guid BookingId) : ICommand<BookingDto>;
