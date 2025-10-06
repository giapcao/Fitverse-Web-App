using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ICoachCertificationRepository : IRepository<CoachCertification>
{
    Task<CoachCertification?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
    Task<IReadOnlyList<CoachCertification>> GetByCoachIdAsync(Guid coachId, CancellationToken ct);
}
