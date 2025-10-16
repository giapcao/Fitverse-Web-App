using Application.CoachTimeoffs.Commands;
using FluentValidation;

namespace Application.CoachTimeoffs.Validators;

public class DeleteCoachTimeoffCommandValidator : AbstractValidator<DeleteCoachTimeoffCommand>
{
    public DeleteCoachTimeoffCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
