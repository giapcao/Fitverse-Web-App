using Application.Authentication.Command;
using FluentValidation;

namespace Application.Authentication.Validator;

public class VerifyPasswordCommandValidator : AbstractValidator<ResetPasswordWithTokenCommand>
{
    public VerifyPasswordCommandValidator()
    {
        RuleFor(x => x.ResetToken).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Phải có ít nhất 1 chữ hoa")
            .Matches(@"[a-z]").WithMessage("Phải có ít nhất 1 chữ thường")
            .Matches(@"\d").WithMessage("Phải có ít nhất 1 số")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Phải có ít nhất 1 ký tự đặc biệt");
    }
}