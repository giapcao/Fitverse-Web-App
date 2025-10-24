using Application.SubscriptionEvents.Commands;
using FluentValidation;

namespace Application.SubscriptionEvents.Validators;

public class UpdateSubscriptionEventCommandValidator : AbstractValidator<UpdateSubscriptionEventCommand>
{
    public UpdateSubscriptionEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.EventType).IsInEnum();
    }
}
