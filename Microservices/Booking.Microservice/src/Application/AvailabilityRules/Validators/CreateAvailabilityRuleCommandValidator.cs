using Application.AvailabilityRules.Commands;
using FluentValidation;

namespace Application.AvailabilityRules.Validators;

public class CreateAvailabilityRuleCommandValidator : AbstractValidator<CreateAvailabilityRuleCommand>
{
    public CreateAvailabilityRuleCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.Weekday).InclusiveBetween(0, 6);
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
        RuleFor(x => x.SlotDurationMinutes).GreaterThan(0);
        RuleFor(x => x.Timezone).NotEmpty().MaximumLength(100);
    }
}
