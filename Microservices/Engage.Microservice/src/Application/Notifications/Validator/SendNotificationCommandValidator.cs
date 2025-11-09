using Application.Notifications.Command;
using FluentValidation;

namespace Application.Notifications.Validator;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Body)
            .NotEmpty()
            .When(command => string.IsNullOrWhiteSpace(command.Title));
        RuleFor(command => command.Title)
            .MaximumLength(200);
        RuleFor(command => command.Data)
            .MaximumLength(2000);
    }
}

