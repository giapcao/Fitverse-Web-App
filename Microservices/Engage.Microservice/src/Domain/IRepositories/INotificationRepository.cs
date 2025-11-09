using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(Guid userId, int take, CancellationToken cancellationToken);

    Task<bool> ExistsForCampaignAsync(Guid campaignId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken);
}

