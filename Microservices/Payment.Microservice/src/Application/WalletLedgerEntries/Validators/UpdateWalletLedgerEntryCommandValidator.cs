using Application.WalletLedgerEntries.Commands;
using FluentValidation;

namespace Application.WalletLedgerEntries.Validators;

public sealed class UpdateWalletLedgerEntryCommandValidator : AbstractValidator<UpdateWalletLedgerEntryCommand>
{
    private const int MaxDescriptionLength = 500;

    public UpdateWalletLedgerEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.JournalId)
            .NotEmpty();

        RuleFor(x => x.WalletId)
            .NotEmpty();

        RuleFor(x => x.AmountVnd)
            .GreaterThan(0);

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Dc)
            .IsInEnum();
    }
}
