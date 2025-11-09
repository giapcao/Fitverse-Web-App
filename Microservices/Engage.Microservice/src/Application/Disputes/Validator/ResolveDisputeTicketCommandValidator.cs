using Application.Disputes.Command;
using Domain.Persistence.Enums;
using FluentValidation;

namespace Application.Disputes.Validator;

public sealed class ResolveDisputeTicketCommandValidator : AbstractValidator<ResolveDisputeTicketCommand>
{
    public ResolveDisputeTicketCommandValidator()
    {
        RuleFor(command => command.DisputeId).NotEmpty();
        RuleFor(command => command.ResolvedBy).NotEmpty();
        RuleFor(command => command.Status)
            .Must(status => status is DisputeStatus.Resolved or DisputeStatus.Dismissed)
            .WithMessage("Status must be resolved or dismissed.");
    }
}

