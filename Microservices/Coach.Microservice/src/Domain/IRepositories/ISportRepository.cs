using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Persistence.Models;
using SharedLibrary.Common;

namespace Domain.IRepositories;

public interface ISportRepository : IRepository<Sport>
{
    Task<Sport?> GetByIdAsync(Guid id, CancellationToken ct, bool asNoTracking = false);
}
