using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ICoachServiceRepository : IRepository<CoachService>
{
    Task<CoachService?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
    Task<IReadOnlyList<CoachService>> GetByCoachIdAsync(Guid coachId, CancellationToken ct);
}
