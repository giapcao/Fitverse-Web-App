using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class WithdrawalRequestRepository : Repository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    public WithdrawalRequestRepository(FitverseDbContext context) : base(context)
    {
    }
}
