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

public class SubscriptionEventRepository : Repository<SubscriptionEvent>, ISubscriptionEventRepository
{
    private readonly FitverseBookingDbContext _context;

    public SubscriptionEventRepository(FitverseBookingDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<SubscriptionEvent?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.SubscriptionEvents.Where(e => e.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionEvent>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.SubscriptionEvents.Where(e => e.SubscriptionId == subscriptionId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionEvent>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.SubscriptionEvents.Where(e => e.BookingId == bookingId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
