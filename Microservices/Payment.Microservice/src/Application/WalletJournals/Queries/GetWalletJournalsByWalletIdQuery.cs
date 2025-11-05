using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletJournals.Queries;

public sealed record GetWalletJournalsByWalletIdQuery(Guid WalletId) : IQuery<IEnumerable<WalletJournalResponse>>;

internal sealed class GetWalletJournalsByWalletIdQueryHandler
    : IQueryHandler<GetWalletJournalsByWalletIdQuery, IEnumerable<WalletJournalResponse>>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IMapper _mapper;

    public GetWalletJournalsByWalletIdQueryHandler(
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IWalletJournalRepository walletJournalRepository,
        IMapper mapper)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _walletJournalRepository = walletJournalRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WalletJournalResponse>>> Handle(
        GetWalletJournalsByWalletIdQuery request,
        CancellationToken cancellationToken)
    {
        var ledgerEntries = await _walletLedgerEntryRepository.FindAsync(
            entry => entry.WalletId == request.WalletId,
            cancellationToken);

        var journalIds = ledgerEntries
            .Select(entry => entry.JournalId)
            .Distinct()
            .ToArray();

        if (journalIds.Length == 0)
        {
            return Result.Success<IEnumerable<WalletJournalResponse>>(Array.Empty<WalletJournalResponse>());
        }

        Expression<Func<WalletJournal, bool>> predicate = journal => journalIds.Contains(journal.Id);
        var journals = await _walletJournalRepository.FindAsync(predicate, cancellationToken);

        var response = _mapper.Map<IEnumerable<WalletJournalResponse>>(journals);
        return Result.Success(response);
    }
}
