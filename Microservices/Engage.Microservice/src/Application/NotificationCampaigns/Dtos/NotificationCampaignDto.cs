using Domain.Persistence.Enums;

namespace Application.NotificationCampaigns.Dtos;

public record NotificationCampaignDto(
    Guid Id,
    string Audience,
    string? TemplateKey,
    string? Title,
    string? Body,
    string? Data,
    DateTime? ScheduledAt,
    CampaignStatus Status,
    Guid? CreatedBy,
    DateTime CreatedAt);

