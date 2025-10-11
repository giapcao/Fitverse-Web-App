using Application.WalletBalances.Commands;
using FluentValidation;

namespace Application.WalletBalances.Validators;

public sealed class UpdateWalletBalanceCommandValidator : AbstractValidator<UpdateWalletBalanceCommand>
{
    public UpdateWalletBalanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.WalletId)
            .NotEmpty();

        RuleFor(x => x.BalanceVnd)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AccountType)
            .IsInEnum();
    }
}
