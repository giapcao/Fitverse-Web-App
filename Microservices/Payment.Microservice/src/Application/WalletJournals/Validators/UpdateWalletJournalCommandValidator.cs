using System;
using Application.WalletJournals.Commands;
using FluentValidation;

namespace Application.WalletJournals.Validators;

public sealed class UpdateWalletJournalCommandValidator : AbstractValidator<UpdateWalletJournalCommand>
{
    public UpdateWalletJournalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.BookingId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Booking id cannot be empty when provided.");

        RuleFor(x => x.PaymentId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Payment id cannot be empty when provided.");

        RuleFor(x => x.PostedAt)
            .Must(value => value != DateTime.MinValue)
            .WithMessage("PostedAt must be a valid date when provided.")
            .When(x => x.PostedAt.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.JournalType)
            .IsInEnum();
    }
}
