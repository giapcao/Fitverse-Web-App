using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication.Validator;

public sealed class VerifyPasswordOtpCommandValidator : AbstractValidator<VerifyPasswordOtpCommand>
{
    public VerifyPasswordOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Otp).NotEmpty().Length(6); 
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Phải có ít nhất 1 chữ hoa")
            .Matches(@"[a-z]").WithMessage("Phải có ít nhất 1 chữ thường")
            .Matches(@"\d").WithMessage("Phải có ít nhất 1 số")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Phải có ít nhất 1 ký tự đặc biệt");
    }
}