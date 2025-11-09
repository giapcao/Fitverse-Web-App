using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Dtos;
using Application.NotificationCampaigns.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.NotificationCampaigns.Handler;

public sealed class GetNotificationCampaignByIdQueryHandler
    : IQueryHandler<GetNotificationCampaignByIdQuery, NotificationCampaignDto>
{
    private readonly INotificationCampaignRepository _notificationCampaignRepository;
    private readonly IMapper _mapper;

    public GetNotificationCampaignByIdQueryHandler(INotificationCampaignRepository notificationCampaignRepository, IMapper mapper)
    {
        _notificationCampaignRepository = notificationCampaignRepository;
        _mapper = mapper;
    }

    public async Task<Result<NotificationCampaignDto>> Handle(GetNotificationCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _notificationCampaignRepository.GetDetailedByIdAsync(request.CampaignId, cancellationToken, asNoTracking: true);
        if (campaign is null)
        {
            return Result.Failure<NotificationCampaignDto>(new Error("NotificationCampaign.NotFound", $"Campaign {request.CampaignId} was not found."));
        }

        return Result.Success(_mapper.Map<NotificationCampaignDto>(campaign));
    }
}

