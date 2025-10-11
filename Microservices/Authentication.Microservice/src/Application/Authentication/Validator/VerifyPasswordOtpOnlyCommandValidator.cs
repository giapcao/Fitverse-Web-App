using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication.Validator;

public sealed class VerifyPasswordOtpOnlyCommandValidator : AbstractValidator<VerifyPasswordOtpOnlyCommand>
{
    public VerifyPasswordOtpOnlyCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Otp)
            .NotEmpty()
            .Length(6);
    }
}

