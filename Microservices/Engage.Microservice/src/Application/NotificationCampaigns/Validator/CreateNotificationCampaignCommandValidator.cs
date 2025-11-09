using Application.NotificationCampaigns.Command;
using FluentValidation;

namespace Application.NotificationCampaigns.Validator;

public sealed class CreateNotificationCampaignCommandValidator : AbstractValidator<CreateNotificationCampaignCommand>
{
    public CreateNotificationCampaignCommandValidator()
    {
        RuleFor(command => command.Audience)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.TemplateKey)
            .MaximumLength(150);

        RuleFor(command => command.Title)
            .MaximumLength(200);

        RuleFor(command => command.Body)
            .NotEmpty()
            .When(command => string.IsNullOrWhiteSpace(command.TemplateKey))
            .WithMessage("Body is required when no template is provided.");
    }
}

