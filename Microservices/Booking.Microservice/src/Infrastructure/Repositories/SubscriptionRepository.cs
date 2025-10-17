using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    private readonly FitverseBookingDbContext _context;

    public SubscriptionRepository(FitverseBookingDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Subscription?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Subscriptions.Where(s => s.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        IQueryable<Subscription> query = _context.Subscriptions
            .Include(s => s.SubscriptionEvents);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Subscriptions.Where(s => s.UserId == userId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(s => s.PeriodStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Subscriptions.Where(s => s.CoachId == coachId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(s => s.PeriodStart)
            .ToListAsync(cancellationToken);
    }
}
