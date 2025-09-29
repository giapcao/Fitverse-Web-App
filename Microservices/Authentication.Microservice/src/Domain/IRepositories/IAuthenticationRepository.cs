using Domain.Entities;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IAuthenticationRepository : IRepository<AppUser>
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
    
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
    Task<AppUser?> FindByEmailAsync(String email, CancellationToken ct, bool asNoTracking = false);
    Task UpdatePasswordHashAsync(Guid userId, string newHash, CancellationToken ct);
}