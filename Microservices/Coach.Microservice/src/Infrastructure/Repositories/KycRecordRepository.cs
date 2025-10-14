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

public class KycRecordRepository : Repository<KycRecord>, IKycRecordRepository
{
    private readonly FitverseCoachDbContext _context;

    public KycRecordRepository(FitverseCoachDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<KycRecord?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.KycRecords
            .Include(r => r.Coach)
            .Where(r => r.Id == id);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<KycRecord>> GetByCoachIdAsync(Guid coachId, CancellationToken ct)
    {
        return await _context.KycRecords
            .Include(r => r.Coach)
            .Where(r => r.CoachId == coachId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<KycRecord>> GetAllDetailedAsync(CancellationToken ct)
    {
        return await _context.KycRecords
            .Include(r => r.Coach)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<KycRecord?> GetLatestByCoachIdAsync(Guid coachId, CancellationToken ct)
    {
        return await _context.KycRecords
            .Include(r => r.Coach)
            .Where(r => r.CoachId == coachId)
            .OrderByDescending(r => r.SubmittedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }
}

