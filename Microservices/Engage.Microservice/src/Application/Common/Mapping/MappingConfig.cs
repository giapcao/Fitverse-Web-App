using Application.Conversations.Dtos;
using Application.Disputes.Dtos;
using Application.NotificationCampaigns.Dtos;
using Application.Notifications.Dtos;
using Application.Reviews.Dtos;
using Domain.Persistence.Models;
using Mapster;

namespace Application.Common.Mapping;

public static class MappingConfig
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<NotificationCampaign, NotificationCampaignDto>();
        config.NewConfig<Notification, NotificationDto>();
        config.NewConfig<Message, MessageDto>();

        config.NewConfig<Conversation, ConversationDto>()
            .Map(dest => dest.Messages, src => src.Messages.OrderBy(message => message.CreatedAt).ToList());

        config.NewConfig<DisputeTicket, DisputeTicketDto>();
        config.NewConfig<Review, ReviewDto>();
    }
}

