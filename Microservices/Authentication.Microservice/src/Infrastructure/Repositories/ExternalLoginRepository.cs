using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ExternalLoginRepository : IExternalLoginRepository
{
    private readonly FitverseDbContext _context;

    public ExternalLoginRepository(FitverseDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalLogin?> FindAsync(string provider, string providerUserId, CancellationToken ct)
    {
        return await _context.ExternalLogins
            .Include(x => x.User)
                .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId, ct);
    }

    public async Task<IReadOnlyList<ExternalLogin>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.ExternalLogins
            .Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(ExternalLogin login, CancellationToken ct)
    {
        await _context.ExternalLogins.AddAsync(login, ct);
    }
}