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
        var q = _context.AppUsers.Where(u => u.Id == id);
        if (asNoTracking) q = q.AsNoTracking();
        return await q.FirstOrDefaultAsync(ct);
    }
    
    public async Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct, bool asNoTracking = false)
    {
        var q = _context.AppUsers.Where(u => u.Email == email);
        if (asNoTracking) q = q.AsNoTracking();
        return await q.FirstOrDefaultAsync(ct);
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string newHash, CancellationToken ct)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;

        user.PasswordHash = newHash;
        user.UpdatedAt = DateTime.UtcNow; 
        await _context.SaveChangesAsync(ct);
    }

}