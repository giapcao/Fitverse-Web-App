using Application.Sports.Command;
using FluentValidation;

namespace Application.Sports.Validator;

public sealed class UpdateSportCommandValidator : AbstractValidator<UpdateSportCommand>
{
    public UpdateSportCommandValidator()
    {
        RuleFor(x => x.SportId).NotEmpty();
        RuleFor(x => x.DisplayName).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
