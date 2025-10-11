using Application.Payments.Commands;
using FluentValidation;

namespace Application.Payments.Validators;

public sealed class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
