using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IKycRecordRepository : IRepository<KycRecord>
{
    Task<KycRecord?> GetDetailedByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
    Task<IReadOnlyList<KycRecord>> GetByCoachIdAsync(Guid coachId, CancellationToken ct);
    Task<KycRecord?> GetLatestByCoachIdAsync(Guid coachId, CancellationToken ct);
}
