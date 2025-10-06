using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication;

public sealed class LoginWithGoogleViaIdTokenCommandValidator : AbstractValidator<LoginWithGoogleViaIdTokenCommand>
{
    public LoginWithGoogleViaIdTokenCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}