using Application.CoachServices.Command;
using FluentValidation;

namespace Application.CoachServices.Validator;

public sealed class CreateCoachServiceCommandValidator : AbstractValidator<CreateCoachServiceCommand>
{
    public CreateCoachServiceCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.SportId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.SessionsTotal).GreaterThan(0);
        RuleFor(x => x.PriceVnd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LocationNote).MaximumLength(500).When(x => x.LocationNote is not null);
    }
}
