using System;
using Application.Users.Command;
using FluentValidation;

namespace Application.Users.Validator;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password!)
                .MinimumLength(8);
        });

        RuleForEach(x => x.RoleIds ?? Array.Empty<string>())
            .NotEmpty().WithMessage("Role id cannot be empty when provided.");
    }
}
