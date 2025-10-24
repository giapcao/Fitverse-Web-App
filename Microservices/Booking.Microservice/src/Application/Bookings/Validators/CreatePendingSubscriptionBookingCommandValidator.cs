using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public class CreatePendingSubscriptionBookingCommandValidator : AbstractValidator<CreatePendingSubscriptionBookingCommand>
{
    public CreatePendingSubscriptionBookingCommandValidator()
    {
        RuleFor(x => x.TimeslotId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.SubscriptionPeriodStart).LessThan(x => x.SubscriptionPeriodEnd);
        RuleFor(x => x.SubscriptionSessionsTotal).GreaterThanOrEqualTo(1);
        RuleFor(x => x.SubscriptionPriceGrossVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SubscriptionCommissionPct).InclusiveBetween(0, 100);
        RuleFor(x => x.SubscriptionCommissionVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SubscriptionNetAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SubscriptionCurrencyCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.BookingGrossAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BookingCommissionPct).InclusiveBetween(0, 100);
        RuleFor(x => x.BookingCommissionVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BookingNetAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BookingCurrencyCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.BookingDurationMinutes)
            .GreaterThan(0)
            .When(x => x.BookingDurationMinutes.HasValue);
    }
}
