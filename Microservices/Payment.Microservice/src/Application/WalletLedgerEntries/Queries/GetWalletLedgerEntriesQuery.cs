using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletLedgerEntries.Queries;

public sealed record GetWalletLedgerEntriesQuery(Guid? WalletId, Guid? JournalId) : IQuery<IEnumerable<WalletLedgerEntryResponse>>;

internal sealed class GetWalletLedgerEntriesQueryHandler : IQueryHandler<GetWalletLedgerEntriesQuery, IEnumerable<WalletLedgerEntryResponse>>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IMapper _mapper;

    public GetWalletLedgerEntriesQueryHandler(IWalletLedgerEntryRepository walletLedgerEntryRepository, IMapper mapper)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WalletLedgerEntryResponse>>> Handle(GetWalletLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<WalletLedgerEntry> entries;

        if (request.WalletId.HasValue || request.JournalId.HasValue)
        {
            Expression<Func<WalletLedgerEntry, bool>> predicate = entry =>
                (!request.WalletId.HasValue || entry.WalletId == request.WalletId) &&
                (!request.JournalId.HasValue || entry.JournalId == request.JournalId);

            entries = await _walletLedgerEntryRepository.FindAsync(predicate, cancellationToken);
        }
        else
        {
            entries = await _walletLedgerEntryRepository.GetAllAsync(cancellationToken);
        }

        var response = _mapper.Map<IEnumerable<WalletLedgerEntryResponse>>(entries);
        return Result.Success(response);
    }
}
