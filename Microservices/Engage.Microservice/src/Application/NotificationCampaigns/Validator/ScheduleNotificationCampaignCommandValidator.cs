using Application.NotificationCampaigns.Command;
using FluentValidation;

namespace Application.NotificationCampaigns.Validator;

public sealed class ScheduleNotificationCampaignCommandValidator : AbstractValidator<ScheduleNotificationCampaignCommand>
{
    public ScheduleNotificationCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId).NotEmpty();
        RuleFor(command => command.ScheduledAt)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Schedule time must be in the future.");
    }
}

