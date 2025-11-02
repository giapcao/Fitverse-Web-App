using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using SharedLibrary.Common;

namespace Domain.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
