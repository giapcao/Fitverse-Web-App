using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.IRepositories;

public interface IExternalLoginRepository
{
    Task<ExternalLogin?> FindAsync(string provider, string providerUserId, CancellationToken ct);
    Task<IReadOnlyList<ExternalLogin>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task AddAsync(ExternalLogin login, CancellationToken ct);
}
