using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence.Models;
using Domain.Persistence;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CoachServiceRepository : Repository<CoachService>, ICoachServiceRepository
{
    private readonly FitverseCoachDbContext _context;

    public CoachServiceRepository(FitverseCoachDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CoachService?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.CoachServices
            .Include(s => s.Coach)
            .Include(s => s.Sport)
            .Where(s => s.Id == id);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CoachService>> GetByCoachIdAsync(Guid coachId, CancellationToken ct)
    {
        return await _context.CoachServices
            .Where(s => s.CoachId == coachId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
