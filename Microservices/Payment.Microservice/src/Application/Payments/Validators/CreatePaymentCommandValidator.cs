using System;
using Application.Payments.Commands;
using FluentValidation;

namespace Application.Payments.Validators;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    private const int MaxGatewayTxnIdLength = 100;

    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty();

        RuleFor(x => x.AmountVnd)
            .GreaterThan(0);

        RuleFor(x => x.GatewayTxnId)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Gateway transaction id cannot be empty when provided.")
            .When(x => x.GatewayTxnId is not null);

        RuleFor(x => x.GatewayTxnId)
            .MaximumLength(MaxGatewayTxnIdLength)
            .When(x => !string.IsNullOrWhiteSpace(x.GatewayTxnId));

        RuleFor(x => x.PaidAt)
            .Must(value => value != DateTime.MinValue)
            .WithMessage("PaidAt must be a valid date when provided.")
            .When(x => x.PaidAt.HasValue);

        RuleFor(x => x.RefundAmountVnd)
            .GreaterThanOrEqualTo(0)
            .When(x => x.RefundAmountVnd.HasValue);

        RuleFor(x => x.RefundAmountVnd)
            .LessThanOrEqualTo(x => x.AmountVnd)
            .When(x => x.RefundAmountVnd.HasValue);

        RuleFor(x => x.Gateway)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
