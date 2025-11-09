using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Dtos;

namespace Application.NotificationCampaigns.Command;

public sealed record CreateNotificationCampaignCommand(
    string Audience,
    string? TemplateKey,
    string? Title,
    string? Body,
    string? Data,
    DateTime? ScheduledAt,
    Guid? CreatedBy) : ICommand<NotificationCampaignDto>;

