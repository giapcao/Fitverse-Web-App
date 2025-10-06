using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CoachProfileRepository : Repository<CoachProfile>, ICoachProfileRepository
{
    private readonly FitverseCoachDbContext _context;

    public CoachProfileRepository(FitverseCoachDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return _context.CoachProfiles.AnyAsync(p => p.UserId == userId, ct);
    }

    public async Task<CoachProfile?> GetDetailedByUserIdAsync(Guid userId, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.CoachProfiles
            .Include(p => p.CoachMedia)
            .Include(p => p.CoachServices)
            .Include(p => p.KycRecords)
            .Include(p => p.Sports)
            .Where(p => p.UserId == userId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CoachProfile>> GetAllDetailedAsync(CancellationToken ct)
    {
        return await _context.CoachProfiles
            .Include(p => p.CoachMedia)
            .Include(p => p.CoachServices)
            .Include(p => p.KycRecords)
            .Include(p => p.Sports)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}

