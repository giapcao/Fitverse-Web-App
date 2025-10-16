using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<Booking?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<Booking>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false);
}
