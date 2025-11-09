using Application.Disputes.Command;
using FluentValidation;

namespace Application.Disputes.Validator;

public sealed class OpenDisputeTicketCommandValidator : AbstractValidator<OpenDisputeTicketCommand>
{
    public OpenDisputeTicketCommandValidator()
    {
        RuleFor(command => command.BookingId).NotEmpty();
        RuleFor(command => command.OpenedBy).NotEmpty();
        RuleFor(command => command.ReasonType)
            .NotEmpty()
            .MaximumLength(150);
    }
}

