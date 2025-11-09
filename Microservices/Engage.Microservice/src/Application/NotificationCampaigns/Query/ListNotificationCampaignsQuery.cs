using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Dtos;
using Domain.Persistence.Enums;

namespace Application.NotificationCampaigns.Query;

public sealed record ListNotificationCampaignsQuery(CampaignStatus? Status) : IQuery<IReadOnlyList<NotificationCampaignDto>>;

