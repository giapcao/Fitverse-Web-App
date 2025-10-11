using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class WalletBalanceRepository : Repository<WalletBalance>, IWalletBalanceRepository
{
    public WalletBalanceRepository(FitverseDbContext context) : base(context)
    {
    }
}
