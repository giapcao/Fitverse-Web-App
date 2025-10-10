using Application.WalletJournals.Commands;
using FluentValidation;

namespace Application.WalletJournals.Validators;

public sealed class DeleteWalletJournalCommandValidator : AbstractValidator<DeleteWalletJournalCommand>
{
    public DeleteWalletJournalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
