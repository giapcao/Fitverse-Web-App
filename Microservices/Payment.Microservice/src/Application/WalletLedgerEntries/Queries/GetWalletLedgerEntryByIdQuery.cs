using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletLedgerEntries.Queries;

public sealed record GetWalletLedgerEntryByIdQuery(Guid Id) : IQuery<WalletLedgerEntryResponse>;

internal sealed class GetWalletLedgerEntryByIdQueryHandler : IQueryHandler<GetWalletLedgerEntryByIdQuery, WalletLedgerEntryResponse>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IMapper _mapper;

    public GetWalletLedgerEntryByIdQueryHandler(IWalletLedgerEntryRepository walletLedgerEntryRepository, IMapper mapper)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletLedgerEntryResponse>> Handle(GetWalletLedgerEntryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _walletLedgerEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            var response = _mapper.Map<WalletLedgerEntryResponse>(entry);
            return Result.Success(response);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletLedgerEntryResponse>(new Error("WalletLedgerEntry.NotFound", $"Wallet ledger entry with id {request.Id} was not found."));
        }
    }
}
