using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.IRepositories;
using Domain.Persistence;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TimeslotRepository : Repository<Timeslot>, ITimeslotRepository
{
    private readonly FitverseBookingDbContext _context;

    public TimeslotRepository(FitverseBookingDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Timeslot?> FindByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Timeslots.Where(t => t.Id == id);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Timeslot?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        IQueryable<Timeslot> query = _context.Timeslots
            .Include(t => t.Bookings)
            .Include(t => t.SubscriptionEvents);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Timeslot>> GetByCoachIdAsync(Guid coachId, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Timeslots.Where(t => t.CoachId == coachId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(t => t.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Timeslot>> GetOpenByCoachAndRangeAsync(Guid coachId, DateTime from, DateTime to, CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var query = _context.Timeslots.Where(t =>
            t.CoachId == coachId &&
            t.Status == SlotStatus.Open &&
            t.StartAt >= from &&
            t.EndAt <= to);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(t => t.StartAt)
            .ToListAsync(cancellationToken);
    }
}
