using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Bookings.Queries;

public sealed record GetBookingByIdQuery(Guid Id) : IQuery<BookingDto>;
