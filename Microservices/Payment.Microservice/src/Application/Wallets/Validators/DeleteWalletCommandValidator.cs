using Application.Wallets.Commands;
using FluentValidation;

namespace Application.Wallets.Validators;

public sealed class DeleteWalletCommandValidator : AbstractValidator<DeleteWalletCommand>
{
    public DeleteWalletCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
