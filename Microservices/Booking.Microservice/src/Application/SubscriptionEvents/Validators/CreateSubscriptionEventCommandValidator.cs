using Application.SubscriptionEvents.Commands;
using FluentValidation;

namespace Application.SubscriptionEvents.Validators;

public class CreateSubscriptionEventCommandValidator : AbstractValidator<CreateSubscriptionEventCommand>
{
    public CreateSubscriptionEventCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.EventType).IsInEnum();
    }
}
