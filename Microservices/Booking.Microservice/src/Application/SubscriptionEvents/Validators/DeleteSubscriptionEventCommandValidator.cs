using Application.SubscriptionEvents.Commands;
using FluentValidation;

namespace Application.SubscriptionEvents.Validators;

public class DeleteSubscriptionEventCommandValidator : AbstractValidator<DeleteSubscriptionEventCommand>
{
    public DeleteSubscriptionEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
