using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ICoachProfileRepository : IRepository<CoachProfile>
{
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct);
    Task<CoachProfile?> GetDetailedByUserIdAsync(Guid userId, CancellationToken ct, bool asNoTracking = false);
    Task<IReadOnlyList<CoachProfile>> GetAllDetailedAsync(CancellationToken ct);
}
