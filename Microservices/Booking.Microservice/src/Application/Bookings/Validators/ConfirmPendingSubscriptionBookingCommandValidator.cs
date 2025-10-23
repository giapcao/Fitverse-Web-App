using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public sealed class ConfirmPendingSubscriptionBookingCommandValidator : AbstractValidator<ConfirmPendingSubscriptionBookingCommand>
{
    public ConfirmPendingSubscriptionBookingCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.BookingId).NotEmpty();
    }
}
