using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

internal static class BookingErrors
{
    internal static Error NotFound(Guid id) => new("Booking.NotFound", $"Booking '{id}' was not found.");
}
