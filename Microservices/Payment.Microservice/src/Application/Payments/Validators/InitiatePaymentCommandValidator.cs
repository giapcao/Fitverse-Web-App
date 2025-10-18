using System;
using Application.Payments.Commands;
using FluentValidation;

namespace Application.Payments.Validators;

public sealed class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.AmountVnd)
            .GreaterThan(0);

        RuleFor(x => x.Gateway)
            .IsInEnum();

        RuleFor(x => x.BookingId)
            .NotEqual(Guid.Empty)
            .When(x => x.BookingId.HasValue);
    }
}
