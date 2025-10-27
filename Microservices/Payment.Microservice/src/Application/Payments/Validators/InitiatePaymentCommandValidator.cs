using System;
using Application.Payments.Commands;
using FluentValidation;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Validators;

public sealed class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.AmountVnd)
            .GreaterThan(0);

        RuleFor(x => x.Gateway)
            .IsInEnum();

        RuleFor(x => x.Flow)
            .IsInEnum();

        RuleFor(x => x.BookingId)
            .NotEqual(Guid.Empty)
            .When(x => x.BookingId.HasValue);

        RuleFor(x => x.BookingId)
            .NotNull()
            .When(x => x.Flow == PaymentFlow.BookingByWallet)
            .WithMessage("BookingId is required for booking flows.");
    }
}
