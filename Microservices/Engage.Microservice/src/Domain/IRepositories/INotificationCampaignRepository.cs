using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface INotificationCampaignRepository : IRepository<NotificationCampaign>
{
    Task<NotificationCampaign?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);

    Task<bool> ExistsByTemplateAndAudienceAsync(string? templateKey, string audience, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationCampaign>> GetByStatusAsync(CampaignStatus? status, CancellationToken cancellationToken);
}

