using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SportRepository : Repository<Sport>, ISportRepository
{
    private readonly FitverseCoachDbContext _context;

    public SportRepository(FitverseCoachDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Sport?> GetByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.Sports.Where(s => s.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }
}
