using Application.CoachProfiles.Command;
using FluentValidation;

namespace Application.CoachProfiles.Validator;

public sealed class UpdateCoachProfileCommandValidator : AbstractValidator<UpdateCoachProfileCommand>
{
    public UpdateCoachProfileCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.YearsExperience).GreaterThanOrEqualTo(0).When(x => x.YearsExperience.HasValue);
        RuleFor(x => x.BasePriceVnd).GreaterThanOrEqualTo(0).When(x => x.BasePriceVnd.HasValue);
        RuleFor(x => x.ServiceRadiusKm).GreaterThan(0).When(x => x.ServiceRadiusKm.HasValue);
    }
}
