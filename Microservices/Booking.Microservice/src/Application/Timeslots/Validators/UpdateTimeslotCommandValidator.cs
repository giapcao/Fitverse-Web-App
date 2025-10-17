using Application.Timeslots.Commands;
using FluentValidation;

namespace Application.Timeslots.Validators;

public class UpdateTimeslotCommandValidator : AbstractValidator<UpdateTimeslotCommand>
{
    public UpdateTimeslotCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.StartAt).LessThan(x => x.EndAt);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Status).IsInEnum();
    }
}
