using Application.CoachServices.Command;
using FluentValidation;

namespace Application.CoachServices.Validator;

public sealed class UpdateCoachServiceCommandValidator : AbstractValidator<UpdateCoachServiceCommand>
{
    public UpdateCoachServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.SportId)
            .NotEmpty()
            .When(x => x.SportId.HasValue);
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.SessionsTotal).GreaterThan(0).When(x => x.SessionsTotal.HasValue);
        RuleFor(x => x.PriceVnd).GreaterThanOrEqualTo(0).When(x => x.PriceVnd.HasValue);
        RuleFor(x => x.LocationNote).MaximumLength(500).When(x => x.LocationNote is not null);
    }
}
