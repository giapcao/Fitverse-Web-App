using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletJournals.Queries;

public sealed record GetWalletJournalByIdQuery(Guid Id) : IQuery<WalletJournalResponse>;

internal sealed class GetWalletJournalByIdQueryHandler : IQueryHandler<GetWalletJournalByIdQuery, WalletJournalResponse>
{
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IMapper _mapper;

    public GetWalletJournalByIdQueryHandler(IWalletJournalRepository walletJournalRepository, IMapper mapper)
    {
        _walletJournalRepository = walletJournalRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletJournalResponse>> Handle(GetWalletJournalByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var journal = await _walletJournalRepository.GetByIdAsync(request.Id, cancellationToken);
            var response = _mapper.Map<WalletJournalResponse>(journal);
            return Result.Success(response);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletJournalResponse>(new Error("WalletJournal.NotFound", $"Wallet journal with id {request.Id} was not found."));
        }
    }
}
