using Application.Timeslots.Commands;
using FluentValidation;

namespace Application.Timeslots.Validators;

public class DeleteTimeslotCommandValidator : AbstractValidator<DeleteTimeslotCommand>
{
    public DeleteTimeslotCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
