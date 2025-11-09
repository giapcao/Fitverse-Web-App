namespace WebApi.Contracts.Campaigns;

public record CreateNotificationCampaignRequest(
    string Audience,
    string? TemplateKey,
    string? Title,
    string? Body,
    string? Data,
    DateTime? ScheduledAt,
    Guid? CreatedBy);

