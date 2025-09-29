using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication.Validator;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ResetBaseUrl).NotEmpty();
    }
}