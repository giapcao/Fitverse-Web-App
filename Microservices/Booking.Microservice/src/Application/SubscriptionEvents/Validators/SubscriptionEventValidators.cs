using Application.SubscriptionEvents.Commands;
using FluentValidation;

namespace Application.SubscriptionEvents.Validators;

public sealed class CreateSubscriptionEventCommandValidator : AbstractValidator<CreateSubscriptionEventCommand>
{
    public CreateSubscriptionEventCommandValidator()
    {
        RuleFor(x => x.SubscriptionEvent.SubscriptionId).NotEmpty();
    }
}

public sealed class UpdateSubscriptionEventCommandValidator : AbstractValidator<UpdateSubscriptionEventCommand>
{
    public UpdateSubscriptionEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SubscriptionEvent.EventType).IsInEnum();
    }
}
