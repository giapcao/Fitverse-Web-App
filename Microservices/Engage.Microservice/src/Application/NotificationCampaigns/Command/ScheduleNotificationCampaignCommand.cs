using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Dtos;

namespace Application.NotificationCampaigns.Command;

public sealed record ScheduleNotificationCampaignCommand(
    Guid CampaignId,
    DateTime ScheduledAt) : ICommand<NotificationCampaignDto>;

