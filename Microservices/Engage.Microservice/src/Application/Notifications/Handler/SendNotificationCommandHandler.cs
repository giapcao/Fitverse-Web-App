using Application.Abstractions.Messaging;
using Application.Notifications.Command;
using Application.Notifications.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Notifications.Handler;

public sealed class SendNotificationCommandHandler
    : ICommandHandler<SendNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationCampaignRepository _notificationCampaignRepository;
    private readonly IMapper _mapper;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        INotificationCampaignRepository notificationCampaignRepository,
        IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _notificationCampaignRepository = notificationCampaignRepository;
        _mapper = mapper;
    }

    public async Task<Result<NotificationDto>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        if (request.CampaignId.HasValue)
        {
            var campaign = await _notificationCampaignRepository.GetByIdAsync(request.CampaignId.Value, cancellationToken);
            if (campaign is null)
            {
                return Result.Failure<NotificationDto>(new Error("NotificationCampaign.NotFound", "Campaign attached to notification was not found."));
            }

            var exists = await _notificationRepository.ExistsForCampaignAsync(
                request.CampaignId.Value,
                request.UserId,
                request.Channel,
                cancellationToken);

            if (exists)
            {
                return Result.Failure<NotificationDto>(new Error("Notification.Exists", "A notification for this user and campaign already exists."));
            }
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Channel = request.Channel,
            Title = request.Title,
            Body = request.Body,
            Data = request.Data,
            SentAt = DateTime.UtcNow,
            CampaignId = request.CampaignId
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);

        return Result.Success(_mapper.Map<NotificationDto>(notification));
    }
}

