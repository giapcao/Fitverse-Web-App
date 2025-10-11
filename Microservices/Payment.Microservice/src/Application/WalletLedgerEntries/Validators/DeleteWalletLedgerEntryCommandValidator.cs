using Application.WalletLedgerEntries.Commands;
using FluentValidation;

namespace Application.WalletLedgerEntries.Validators;

public sealed class DeleteWalletLedgerEntryCommandValidator : AbstractValidator<DeleteWalletLedgerEntryCommand>
{
    public DeleteWalletLedgerEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
