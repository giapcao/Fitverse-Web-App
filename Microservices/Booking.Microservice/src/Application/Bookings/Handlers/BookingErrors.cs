using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.Bookings.Handlers;

internal static class BookingErrors
{
    internal static Error NotFound(Guid id) => new("Booking.NotFound", $"Booking '{id}' was not found.");

    internal static Error SubscriptionUnavailable(Guid userId, Guid coachId) =>
        new("Booking.SubscriptionUnavailable", $"No active subscription for user '{userId}' with coach '{coachId}' covers the requested timeslot.");

    internal static Error SubscriptionSessionsExhausted(Guid subscriptionId) =>
        new("Booking.SubscriptionSessionsExhausted", $"Subscription '{subscriptionId}' has no remaining reserved sessions.");

    internal static Error TimeslotRequired(Guid userId, Guid coachId) =>
        new("Booking.TimeslotRequired", $"A timeslot is required to reserve a booking for user '{userId}' with coach '{coachId}'.");

    internal static Error TimeslotNotFound(Guid timeslotId) =>
        new("Booking.TimeslotNotFound", $"Timeslot '{timeslotId}' was not found.");

    internal static Error TimeslotCoachMismatch(Guid timeslotId, Guid coachId) =>
        new("Booking.TimeslotCoachMismatch", $"Timeslot '{timeslotId}' does not belong to coach '{coachId}'.");

    internal static Error TimeslotNotOpen(Guid timeslotId) =>
        new("Booking.TimeslotNotOpen", $"Timeslot '{timeslotId}' is no longer open for reservation.");

    internal static Error TimeslotOutsideSubscription(Guid timeslotId, Guid subscriptionId) =>
        new("Booking.TimeslotOutsideSubscription", $"Timeslot '{timeslotId}' falls outside the active period of subscription '{subscriptionId}'.");

    internal static Error SubscriptionInsufficientSessions(Guid subscriptionId, int requestedSessions, int remainingSessions) =>
        new("Booking.SubscriptionInsufficientSessions",
            $"Subscription '{subscriptionId}' does not have enough remaining sessions to reserve {requestedSessions}. Only {remainingSessions} sessions remain.");

    internal static Error NoTimeslotsProvided() =>
        new("Booking.NoTimeslotsProvided", "At least one timeslot must be supplied.");

    internal static Error SubscriptionNotFound(Guid subscriptionId) =>
        new("Booking.SubscriptionNotFound", $"Subscription '{subscriptionId}' was not found.");

    internal static Error SubscriptionNotPending(Guid subscriptionId) =>
        new("Booking.SubscriptionNotPending", $"Subscription '{subscriptionId}' is not pending and cannot be cancelled.");

    internal static Error BookingNotPendingPayment(Guid bookingId) =>
        new("Booking.BookingNotPendingPayment", $"Booking '{bookingId}' is not awaiting payment.");

    internal static Error BookingSubscriptionMismatch(Guid bookingId, Guid subscriptionId) =>
        new("Booking.BookingSubscriptionMismatch", $"Booking '{bookingId}' does not belong to subscription '{subscriptionId}'.");

    internal static Error BookingTimeslotMissing(Guid bookingId) =>
        new("Booking.BookingTimeslotMissing", $"Booking '{bookingId}' does not reference a timeslot.");
}
