using Application.Bookings.Commands;
using FluentValidation;

namespace Application.Bookings.Validators;

public class UpdateBookingCommandValidator : AbstractValidator<UpdateBookingCommand>
{
    public UpdateBookingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndAt);
        RuleFor(x => x.GrossAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommissionPct).InclusiveBetween(0, 100);
        RuleFor(x => x.CommissionVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.NetAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Status).IsInEnum();
    }
}
