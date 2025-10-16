using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IAvailabilityRuleRepository : IRepository<AvailabilityRule>
{
    Task<IReadOnlyList<AvailabilityRule>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<AvailabilityRule?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
}
