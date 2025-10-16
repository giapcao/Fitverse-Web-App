using Application.Subscriptions.Commands;
using FluentValidation;

namespace Application.Subscriptions.Validators;

public class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.PeriodStart).LessThan(x => x.PeriodEnd);
        RuleFor(x => x.SessionsTotal).GreaterThanOrEqualTo(1);
        RuleFor(x => x.SessionsReserved).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SessionsConsumed).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceGrossVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommissionPct).InclusiveBetween(0, 100);
        RuleFor(x => x.CommissionVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.NetAmountVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Status).IsInEnum();
    }
}
