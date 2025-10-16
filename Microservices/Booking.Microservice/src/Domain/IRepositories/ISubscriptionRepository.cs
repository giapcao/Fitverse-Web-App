using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<Subscription?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<Subscription>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false);
}
