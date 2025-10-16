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

public class CoachTimeoffRepository : Repository<CoachTimeoff>, ICoachTimeoffRepository
{
    private readonly FitverseBookingDbContext _context;

    public CoachTimeoffRepository(FitverseBookingDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CoachTimeoff>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.CoachTimeoffs.Where(t => t.CoachId == coachId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(t => t.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CoachTimeoff?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.CoachTimeoffs.Where(t => t.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
