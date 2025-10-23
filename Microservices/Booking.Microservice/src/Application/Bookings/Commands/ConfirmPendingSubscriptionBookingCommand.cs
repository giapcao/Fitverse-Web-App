using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Bookings.Commands;

public sealed record ConfirmPendingSubscriptionBookingCommand(
    Guid SubscriptionId,
    Guid BookingId) : ICommand<BookingDto>;
