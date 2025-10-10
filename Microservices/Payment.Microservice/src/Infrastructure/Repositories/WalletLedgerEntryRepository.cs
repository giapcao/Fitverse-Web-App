using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class WalletLedgerEntryRepository : Repository<WalletLedgerEntry>, IWalletLedgerEntryRepository
{
    public WalletLedgerEntryRepository(FitverseDbContext context) : base(context)
    {
    }
}
