using Domain.Persistence.Enums;

namespace Application.Notifications.Dtos;

public record NotificationDto(
    Guid Id,
    Guid UserId,
    NotificationChannel Channel,
    string? Title,
    string? Body,
    string? Data,
    DateTime SentAt,
    Guid? CampaignId);

