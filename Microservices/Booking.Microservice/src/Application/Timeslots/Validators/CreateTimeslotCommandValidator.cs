using Application.Timeslots.Commands;
using FluentValidation;

namespace Application.Timeslots.Validators;

public class CreateTimeslotCommandValidator : AbstractValidator<CreateTimeslotCommand>
{
    public CreateTimeslotCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndAt);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Status).IsInEnum();
    }
}
