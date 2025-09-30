using System.Collections.Generic;
using Domain.Entities;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IAuthenticationRepository : IRepository<AppUser>
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
    Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct, bool asNoTracking = false);
    Task UpdatePasswordHashAsync(Guid userId, string newHash, CancellationToken ct);
    Task<IReadOnlyList<AppUser>> GetAllDetailedAsync(CancellationToken ct);
    Task<AppUser?> GetDetailedByIdAsync(Guid id, CancellationToken ct);
}
