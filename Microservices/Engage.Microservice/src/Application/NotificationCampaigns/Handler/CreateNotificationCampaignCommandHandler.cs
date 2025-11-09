using Application.Abstractions.Messaging;
using Application.NotificationCampaigns.Command;
using Application.NotificationCampaigns.Dtos;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.NotificationCampaigns.Handler;

public sealed class CreateNotificationCampaignCommandHandler
    : ICommandHandler<CreateNotificationCampaignCommand, NotificationCampaignDto>
{
    private readonly INotificationCampaignRepository _notificationCampaignRepository;
    private readonly IMapper _mapper;

    public CreateNotificationCampaignCommandHandler(INotificationCampaignRepository notificationCampaignRepository, IMapper mapper)
    {
        _notificationCampaignRepository = notificationCampaignRepository;
        _mapper = mapper;
    }

    public async Task<Result<NotificationCampaignDto>> Handle(CreateNotificationCampaignCommand request, CancellationToken cancellationToken)
    {
        var audience = request.Audience.Trim();
        var templateKey = request.TemplateKey?.Trim();

        var exists = await _notificationCampaignRepository.ExistsByTemplateAndAudienceAsync(templateKey, audience, cancellationToken);
        if (exists)
        {
            return Result.Failure<NotificationCampaignDto>(new Error("NotificationCampaign.Exists", "A campaign with the same audience/template already exists."));
        }

        var utcNow = DateTime.UtcNow;
        var campaign = new NotificationCampaign
        {
            Id = Guid.NewGuid(),
            Audience = audience,
            TemplateKey = templateKey,
            Title = request.Title?.Trim(),
            Body = request.Body,
            Data = request.Data,
            ScheduledAt = request.ScheduledAt,
            Status = request.ScheduledAt.HasValue && request.ScheduledAt <= utcNow ? CampaignStatus.Running :
                request.ScheduledAt.HasValue ? CampaignStatus.Scheduled : CampaignStatus.Draft,
            CreatedBy = request.CreatedBy,
            CreatedAt = utcNow
        };

        await _notificationCampaignRepository.AddAsync(campaign, cancellationToken);

        var dto = _mapper.Map<NotificationCampaignDto>(campaign);
        return Result.Success(dto);
    }
}

