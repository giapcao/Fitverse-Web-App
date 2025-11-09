using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Review>> GetCoachReviewsAsync(Guid coachId, CancellationToken cancellationToken);
}

