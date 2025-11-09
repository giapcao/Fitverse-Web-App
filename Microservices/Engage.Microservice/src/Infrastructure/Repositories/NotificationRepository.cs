using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    private readonly FitverseEngageDbContext _context;

    public NotificationRepository(FitverseEngageDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(Guid userId, int take, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.SentAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForCampaignAsync(Guid campaignId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken)
    {
        return _context.Notifications.AnyAsync(notification =>
            notification.CampaignId == campaignId &&
            notification.UserId == userId &&
            notification.Channel == channel, cancellationToken);
    }
}

