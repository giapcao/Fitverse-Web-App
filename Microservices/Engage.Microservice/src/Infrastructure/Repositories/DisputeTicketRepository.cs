using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DisputeTicketRepository : Repository<DisputeTicket>, IDisputeTicketRepository
{
    private readonly FitverseEngageDbContext _context;

    public DisputeTicketRepository(FitverseEngageDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return _context.DisputeTickets.AnyAsync(ticket => ticket.BookingId == bookingId, cancellationToken);
    }

    public async Task<IReadOnlyList<DisputeTicket>> GetByStatusAsync(DisputeStatus? status, CancellationToken cancellationToken)
    {
        var query = _context.DisputeTickets.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(ticket => ticket.Status == status);
        }

        return await query
            .OrderByDescending(ticket => ticket.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

