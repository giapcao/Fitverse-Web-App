using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    private readonly FitverseBookingDbContext _context;

    public BookingRepository(FitverseBookingDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Booking?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Bookings.Where(b => b.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Booking?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        IQueryable<Booking> query = _context.Bookings
            .Include(b => b.Timeslot)
            .Include(b => b.SubscriptionEvents);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Bookings.Where(b => b.UserId == userId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(b => b.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Bookings.Where(b => b.CoachId == coachId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderByDescending(b => b.StartAt)
            .ToListAsync(cancellationToken);
    }
}
