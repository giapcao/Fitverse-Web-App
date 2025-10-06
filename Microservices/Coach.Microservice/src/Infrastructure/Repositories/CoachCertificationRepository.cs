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

public class CoachCertificationRepository : Repository<CoachCertification>, ICoachCertificationRepository
{
    private readonly FitverseCoachDbContext _context;

    public CoachCertificationRepository(FitverseCoachDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CoachCertification?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.CoachCertifications.Where(c => c.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CoachCertification>> GetByCoachIdAsync(Guid coachId, CancellationToken ct)
    {
        return await _context.CoachCertifications
            .Where(c => c.CoachId == coachId)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
