using Application.Bookings.Commands;
using FluentValidation;
using SharedLibrary.Contracts.Payments;

namespace Application.Bookings.Validators;

public class CreatePendingSubscriptionBookingCommandValidator : AbstractValidator<CreatePendingSubscriptionBookingCommand>
{
    public CreatePendingSubscriptionBookingCommandValidator()
    {
        RuleFor(x => x.TimeslotId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.SubscriptionSessionsTotal).GreaterThanOrEqualTo(1);
        RuleFor(x => x.SubscriptionCommissionPct).InclusiveBetween(0, 100);
        RuleFor(x => x.SubscriptionNetAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BookingDurationMinutes)
            .GreaterThan(0)
            .When(x => x.BookingDurationMinutes.HasValue);
        RuleFor(x => x.Gateway).IsInEnum();
        RuleFor(x => x.Flow).IsInEnum();
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .When(x => x.Flow == PaymentFlow.BookingByWallet);
    }
}
