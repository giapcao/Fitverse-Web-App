using Application.CoachTimeoffs.Commands;
using FluentValidation;

namespace Application.CoachTimeoffs.Validators;

public class CreateCoachTimeoffCommandValidator : AbstractValidator<CreateCoachTimeoffCommand>
{
    public CreateCoachTimeoffCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndAt);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
