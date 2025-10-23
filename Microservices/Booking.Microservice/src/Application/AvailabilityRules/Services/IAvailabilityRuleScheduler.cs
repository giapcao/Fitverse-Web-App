using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;

namespace Application.AvailabilityRules.Services;

public interface IAvailabilityRuleScheduler
{
    Task RemoveFutureOpenSlotsAsync(AvailabilityRule rule, CancellationToken cancellationToken);
    Task EnsureFutureSlotsAsync(AvailabilityRule rule, CancellationToken cancellationToken);
}
