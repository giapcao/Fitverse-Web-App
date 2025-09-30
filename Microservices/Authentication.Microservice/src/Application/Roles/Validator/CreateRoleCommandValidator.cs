using Application.Roles.Command;
using FluentValidation;

namespace Application.Roles.Validator;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(128);
    }
}
