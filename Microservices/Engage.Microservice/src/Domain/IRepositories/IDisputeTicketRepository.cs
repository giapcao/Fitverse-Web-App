using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface IDisputeTicketRepository : IRepository<DisputeTicket>
{
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DisputeTicket>> GetByStatusAsync(DisputeStatus? status, CancellationToken cancellationToken);
}

