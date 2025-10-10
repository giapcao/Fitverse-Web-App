using Application.WalletBalances.Commands;
using FluentValidation;

namespace Application.WalletBalances.Validators;

public sealed class CreateWalletBalanceCommandValidator : AbstractValidator<CreateWalletBalanceCommand>
{
    public CreateWalletBalanceCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty();

        RuleFor(x => x.BalanceVnd)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AccountType)
            .IsInEnum();
    }
}
