using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class NotificationCampaignRepository : Repository<NotificationCampaign>, INotificationCampaignRepository
{
    private readonly FitverseEngageDbContext _context;

    public NotificationCampaignRepository(FitverseEngageDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<NotificationCampaign?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.NotificationCampaigns
            .Include(campaign => campaign.Notifications)
            .Where(campaign => campaign.Id == id);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExistsByTemplateAndAudienceAsync(string? templateKey, string audience, CancellationToken cancellationToken)
    {
        return _context.NotificationCampaigns
            .AnyAsync(campaign =>
                campaign.Audience == audience &&
                (templateKey == null || campaign.TemplateKey == templateKey),
                cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationCampaign>> GetByStatusAsync(CampaignStatus? status, CancellationToken cancellationToken)
    {
        var query = _context.NotificationCampaigns.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(campaign => campaign.Status == status);
        }

        return await query
            .OrderByDescending(campaign => campaign.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

