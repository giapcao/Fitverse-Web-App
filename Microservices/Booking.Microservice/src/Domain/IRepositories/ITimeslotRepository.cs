using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ITimeslotRepository : IRepository<Timeslot>
{
    Task<Timeslot?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<Timeslot?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<Timeslot>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<Timeslot>> GetOpenByCoachAndRangeAsync(Guid coachId, DateTime from, DateTime to, CancellationToken cancellationToken, bool asNoTracking = false);
}
