using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ICoachMediaRepository : IRepository<CoachMedium>
{
    Task<CoachMedium?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
    Task<IReadOnlyList<CoachMedium>> GetByCoachIdAsync(Guid coachId, CancellationToken ct);
}
