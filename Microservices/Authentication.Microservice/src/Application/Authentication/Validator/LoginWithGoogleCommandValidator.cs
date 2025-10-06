using System;
using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication;

public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.State).NotEmpty();
        RuleFor(x => x.RedirectUri)
            .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("RedirectUri must be an absolute URI when provided.");
    }
}
