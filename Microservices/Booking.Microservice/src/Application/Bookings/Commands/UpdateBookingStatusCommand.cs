using System;
using Application.Abstractions.Messaging;
using Application.Features;
using Domain.Persistence.Enums;

namespace Application.Bookings.Commands;

public sealed record UpdateBookingStatusCommand(Guid BookingId, BookingStatus RequestedStatus) : ICommand<BookingDto>;
