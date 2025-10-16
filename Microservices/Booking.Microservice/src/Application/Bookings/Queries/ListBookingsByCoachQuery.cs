using System;
using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Bookings.Queries;

public sealed record ListBookingsByCoachQuery(Guid CoachId) : IQuery<IReadOnlyCollection<BookingDto>>;
