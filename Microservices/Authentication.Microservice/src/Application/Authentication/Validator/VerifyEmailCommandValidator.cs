using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication.Validator;

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
    }
}

