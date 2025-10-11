using Application.Roles.Command;
using FluentValidation;

namespace Application.Roles.Validator;

public sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}

