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

public class AvailabilityRuleRepository : Repository<AvailabilityRule>, IAvailabilityRuleRepository
{
    private readonly FitverseBookingDbContext _context;

    public AvailabilityRuleRepository(FitverseBookingDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AvailabilityRule>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.AvailabilityRules.Where(rule => rule.CoachId == coachId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(rule => rule.Weekday)
            .ThenBy(rule => rule.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<AvailabilityRule?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.AvailabilityRules.Where(rule => rule.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
