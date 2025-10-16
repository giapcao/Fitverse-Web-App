using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ICoachTimeoffRepository : IRepository<CoachTimeoff>
{
    Task<IReadOnlyList<CoachTimeoff>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<CoachTimeoff?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
}
