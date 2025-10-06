using Application.CoachServices.Command;
using FluentValidation;

namespace Application.CoachServices.Validator;

public sealed class DeleteCoachServiceCommandValidator : AbstractValidator<DeleteCoachServiceCommand>
{
    public DeleteCoachServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
    }
}
