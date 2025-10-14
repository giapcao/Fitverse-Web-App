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

public class CoachMediaRepository : Repository<CoachMedium>, ICoachMediaRepository
{
    private readonly FitverseCoachDbContext _context;

    public CoachMediaRepository(FitverseCoachDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CoachMedium?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.CoachMedia
            .Include(m => m.Coach)
            .Where(m => m.Id == id);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CoachMedium>> GetByCoachIdAsync(Guid coachId, CancellationToken ct)
    {
        return await _context.CoachMedia
            .Where(m => m.CoachId == coachId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CoachMedium>> GetFeaturedByCoachIdAsync(Guid coachId, bool isFeatured, CancellationToken ct)
    {
        return await _context.CoachMedia
            .Where(m => m.CoachId == coachId && m.IsFeatured == isFeatured)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CoachMedium>> GetFeaturedAsync(bool isFeatured, CancellationToken ct)
    {
        return await _context.CoachMedia
            .Where(m => m.IsFeatured == isFeatured)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
