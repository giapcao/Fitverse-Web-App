using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Dtos;
using Application.NotificationCampaigns.Query;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.NotificationCampaigns.Handler;

public sealed class ListNotificationCampaignsQueryHandler
    : IQueryHandler<ListNotificationCampaignsQuery, IReadOnlyList<NotificationCampaignDto>>
{
    private readonly INotificationCampaignRepository _notificationCampaignRepository;
    private readonly IMapper _mapper;

    public ListNotificationCampaignsQueryHandler(INotificationCampaignRepository notificationCampaignRepository, IMapper mapper)
    {
        _notificationCampaignRepository = notificationCampaignRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<NotificationCampaignDto>>> Handle(ListNotificationCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _notificationCampaignRepository.GetByStatusAsync(request.Status, cancellationToken);
        var dtos = campaigns
            .Select(campaign => _mapper.Map<NotificationCampaignDto>(campaign))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<NotificationCampaignDto>>(dtos);
    }
}

