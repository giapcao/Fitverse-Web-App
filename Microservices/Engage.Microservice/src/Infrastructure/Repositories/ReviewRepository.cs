using Domain.IRepositories;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    private readonly FitverseEngageDbContext _context;

    public ReviewRepository(FitverseEngageDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return _context.Reviews.AnyAsync(review => review.BookingId == bookingId, cancellationToken);
    }

    public Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return _context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(review => review.BookingId == bookingId, cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetCoachReviewsAsync(Guid coachId, CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Where(review => review.CoachId == coachId)
            .OrderByDescending(review => review.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

