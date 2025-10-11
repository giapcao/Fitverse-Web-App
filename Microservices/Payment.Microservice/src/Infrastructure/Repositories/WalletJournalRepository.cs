using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class WalletJournalRepository : Repository<WalletJournal>, IWalletJournalRepository
{
    public WalletJournalRepository(FitverseDbContext context) : base(context)
    {
    }
}
