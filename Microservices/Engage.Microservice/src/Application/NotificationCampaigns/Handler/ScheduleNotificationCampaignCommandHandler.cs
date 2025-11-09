using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Command;
using Application.NotificationCampaigns.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.NotificationCampaigns.Handler;

public sealed class ScheduleNotificationCampaignCommandHandler
    : ICommandHandler<ScheduleNotificationCampaignCommand, NotificationCampaignDto>
{
    private readonly INotificationCampaignRepository _notificationCampaignRepository;
    private readonly IMapper _mapper;

    public ScheduleNotificationCampaignCommandHandler(INotificationCampaignRepository notificationCampaignRepository, IMapper mapper)
    {
        _notificationCampaignRepository = notificationCampaignRepository;
        _mapper = mapper;
    }

    public async Task<Result<NotificationCampaignDto>> Handle(ScheduleNotificationCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _notificationCampaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        var utcNow = DateTime.UtcNow;

        if (campaign is null)
        {
            return Result.Failure<NotificationCampaignDto>(new Error("NotificationCampaign.NotFound", $"Campaign {request.CampaignId} was not found."));
        }

        if (campaign.Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
        {
            return Result.Failure<NotificationCampaignDto>(new Error("NotificationCampaign.Locked", "Completed or cancelled campaigns cannot be rescheduled."));
        }

        campaign.ScheduledAt = request.ScheduledAt;
        campaign.Status = request.ScheduledAt <= utcNow ? CampaignStatus.Running : CampaignStatus.Scheduled;

        _notificationCampaignRepository.Update(campaign);

        var dto = _mapper.Map<NotificationCampaignDto>(campaign);
        return Result.Success(dto);
    }
}

