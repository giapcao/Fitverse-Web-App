using Application.Sports.Command;
using FluentValidation;

namespace Application.Sports.Validator;

public sealed class DeleteSportCommandValidator : AbstractValidator<DeleteSportCommand>
{
    public DeleteSportCommandValidator()
    {
        RuleFor(x => x.SportId).NotEmpty();
    }
}
