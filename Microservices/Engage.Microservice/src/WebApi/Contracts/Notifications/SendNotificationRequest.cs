using Domain.Persistence.Enums;

namespace WebApi.Contracts.Notifications;

public record SendNotificationRequest(
    Guid UserId,
    NotificationChannel Channel,
    string? Title,
    string? Body,
    string? Data,
    Guid? CampaignId);

