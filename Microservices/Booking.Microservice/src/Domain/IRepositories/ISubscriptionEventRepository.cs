using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ISubscriptionEventRepository : IRepository<SubscriptionEvent>
{
    Task<SubscriptionEvent?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<SubscriptionEvent>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken, bool asNoTracking = false);
    Task<IReadOnlyList<SubscriptionEvent>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken, bool asNoTracking = false);
}
