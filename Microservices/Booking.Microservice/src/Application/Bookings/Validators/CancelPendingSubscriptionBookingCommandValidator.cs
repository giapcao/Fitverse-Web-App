using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public sealed class CancelPendingSubscriptionBookingCommandValidator : AbstractValidator<CancelPendingSubscriptionBookingCommand>
{
    public CancelPendingSubscriptionBookingCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.BookingId).NotEmpty();
    }
}
