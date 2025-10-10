using Application.WalletBalances.Commands;
using FluentValidation;

namespace Application.WalletBalances.Validators;

public sealed class DeleteWalletBalanceCommandValidator : AbstractValidator<DeleteWalletBalanceCommand>
{
    public DeleteWalletBalanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
