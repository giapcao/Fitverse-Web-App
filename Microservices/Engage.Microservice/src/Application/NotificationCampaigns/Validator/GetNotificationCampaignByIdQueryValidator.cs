using Application.NotificationCampaigns.Query;
using FluentValidation;

namespace Application.NotificationCampaigns.Validator;

public sealed class GetNotificationCampaignByIdQueryValidator : AbstractValidator<GetNotificationCampaignByIdQuery>
{
    public GetNotificationCampaignByIdQueryValidator()
    {
        RuleFor(query => query.CampaignId).NotEmpty();
    }
}

