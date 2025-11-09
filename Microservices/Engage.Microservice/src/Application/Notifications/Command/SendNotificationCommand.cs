using Application.Abstractions.Messaging;
using Application.Notifications.Dtos;
using Domain.Persistence.Enums;

namespace Application.Notifications.Command;

public sealed record SendNotificationCommand(
    Guid UserId,
    NotificationChannel Channel,
    string? Title,
    string? Body,
    string? Data,
    Guid? CampaignId) : ICommand<NotificationDto>;

