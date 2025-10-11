using Application.Wallets.Commands;
using FluentValidation;

namespace Application.Wallets.Validators;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
