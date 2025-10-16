using Application.AvailabilityRules.Commands;
using FluentValidation;

namespace Application.AvailabilityRules.Validators;

public class DeleteAvailabilityRuleCommandValidator : AbstractValidator<DeleteAvailabilityRuleCommand>
{
    public DeleteAvailabilityRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
