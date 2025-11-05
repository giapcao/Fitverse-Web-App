using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.WalletJournals.Queries;
using Application.WalletLedgerEntries.Queries;
using Domain.Entities;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Wallets.Queries;

public sealed record GetWalletHistoryByWalletIdQuery(Guid WalletId) : IQuery<WalletHistoryResponse>;

internal sealed class GetWalletHistoryByWalletIdQueryHandler
    : IQueryHandler<GetWalletHistoryByWalletIdQuery, WalletHistoryResponse>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IMapper _mapper;

    public GetWalletHistoryByWalletIdQueryHandler(
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IWalletJournalRepository walletJournalRepository,
        IMapper mapper)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _walletJournalRepository = walletJournalRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletHistoryResponse>> Handle(
        GetWalletHistoryByWalletIdQuery request,
        CancellationToken cancellationToken)
    {
        var ledgerEntries = (await _walletLedgerEntryRepository.FindAsync(
                entry => entry.WalletId == request.WalletId,
                cancellationToken))
            .OrderByDescending(entry => entry.CreatedAt)
            .ToList();

        if (ledgerEntries.Count == 0)
        {
            var emptyHistory = new WalletHistoryResponse(
                request.WalletId,
                Array.Empty<WalletHistoryItemResponse>());

            return Result.Success(emptyHistory);
        }

        var ledgerResponses = _mapper.Map<List<WalletLedgerEntryResponse>>(ledgerEntries);
        var ledgerGroups = ledgerResponses
            .GroupBy(entry => entry.JournalId)
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<WalletLedgerEntryResponse>)group.ToList());

        var journalIds = ledgerGroups.Keys.ToArray();
        Expression<Func<WalletJournal, bool>> predicate = journal => journalIds.Contains(journal.Id);
        var journals = await _walletJournalRepository.FindAsync(predicate, cancellationToken);
        var journalResponses = _mapper.Map<List<WalletJournalResponse>>(journals)
            .OrderByDescending(journal => journal.CreatedAt)
            .ToList();

        var items = journalResponses
            .Select(journal =>
            {
                ledgerGroups.TryGetValue(journal.Id, out var entries);
                entries ??= Array.Empty<WalletLedgerEntryResponse>();

                var journalHistory = new WalletJournalHistoryResponse(
                    journal.Id,
                    journal.BookingId,
                    journal.PaymentId,
                    journal.Status,
                    journal.JournalType,
                    journal.CreatedAt,
                    journal.PostedAt,
                    entries);

                return new WalletHistoryItemResponse(journalHistory);
            })
            .ToList();

        var history = new WalletHistoryResponse(request.WalletId, items);

        return Result.Success(history);
    }
}
