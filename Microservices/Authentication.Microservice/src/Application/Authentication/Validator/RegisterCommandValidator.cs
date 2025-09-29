using System.Windows.Input;
using Application.Abstractions.Messaging;
using FluentValidation;
using MediatR;

namespace Application.Authentication.Validator;

public sealed record RegisterCommand(string Email, string Password, string FullName, string ConfirmBaseUrl)
    : ICommand<Unit>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Phải có ít nhất 1 chữ hoa")
            .Matches(@"[a-z]").WithMessage("Phải có ít nhất 1 chữ thường")
            .Matches(@"\d").WithMessage("Phải có ít nhất 1 số")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Phải có ít nhất 1 ký tự đặc biệt");
        RuleFor(x => x.FullName)
            .NotEmpty().MaximumLength(100);
        RuleFor(x => x.ConfirmBaseUrl)
            .NotEmpty().Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("ConfirmBaseUrl phải là URL hợp lệ (absolute).");
    }
}