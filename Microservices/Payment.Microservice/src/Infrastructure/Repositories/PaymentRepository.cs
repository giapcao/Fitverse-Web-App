using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    private readonly FitverseDbContext _context;

    public PaymentRepository(FitverseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var paymentsQuery =
            from ledger in _context.WalletLedgerEntries.AsNoTracking()
            join wallet in _context.Wallets.AsNoTracking() on ledger.WalletId equals wallet.Id
            join journal in _context.WalletJournals.AsNoTracking() on ledger.JournalId equals journal.Id
            join payment in _context.Payments.AsNoTracking() on journal.PaymentId equals (Guid?)payment.Id
            where wallet.UserId == userId
            select payment;

        return await paymentsQuery
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
