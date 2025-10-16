using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Common;

namespace Infrastructure.Repositories;

public class AuthenticationRepository : Repository<AppUser>, IAuthenticationRepository
{
    private readonly FitverseDbContext _context;
    public AuthenticationRepository(FitverseDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _context.AppUsers.AnyAsync(u => u.Email == email, ct);

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.AppUsers
            .Include(u => u.Roles)
            .Where(u => u.Id == id);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct, bool asNoTracking = false)
    {
        var query = _context.AppUsers
            .Include(u => u.Roles)
            .Where(u => u.Email == email);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string newHash, CancellationToken ct)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return;
        }

        user.PasswordHash = newHash;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<IReadOnlyList<AppUser>> GetAllDetailedAsync(CancellationToken ct)
    {
        return await _context.AppUsers
            .Include(u => u.Roles)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<AppUser?> GetDetailedByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.AppUsers
            .Include(u => u.Roles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}

