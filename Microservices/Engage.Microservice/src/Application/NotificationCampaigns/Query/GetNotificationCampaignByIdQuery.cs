using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Dtos;

namespace Application.NotificationCampaigns.Query;

public sealed record GetNotificationCampaignByIdQuery(Guid CampaignId) : IQuery<NotificationCampaignDto>;

