using Application.Sports.Command;
using FluentValidation;

namespace Application.Sports.Validator;

public sealed class CreateSportCommandValidator : AbstractValidator<CreateSportCommand>
{
    public CreateSportCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
