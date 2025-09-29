using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication.Validator;

public sealed class RequestPasswordOtpCommandValidator : AbstractValidator<RequestPasswordOtpCommand>
{
    public RequestPasswordOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
    }
}