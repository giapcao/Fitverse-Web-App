using System;
using Application.Users.Command;
using FluentValidation;

namespace Application.Users.Validator;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleForEach(x => x.RoleIds ?? Array.Empty<Guid>())
            .Must(id => id != Guid.Empty)
            .WithMessage("Role id cannot be empty when provided.");
    }
}
