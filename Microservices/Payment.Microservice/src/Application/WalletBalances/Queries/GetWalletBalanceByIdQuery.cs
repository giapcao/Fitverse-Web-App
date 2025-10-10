using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletBalances.Queries;

public sealed record GetWalletBalanceByIdQuery(Guid Id) : IQuery<WalletBalanceResponse>;

internal sealed class GetWalletBalanceByIdQueryHandler : IQueryHandler<GetWalletBalanceByIdQuery, WalletBalanceResponse>
{
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IMapper _mapper;

    public GetWalletBalanceByIdQueryHandler(IWalletBalanceRepository walletBalanceRepository, IMapper mapper)
    {
        _walletBalanceRepository = walletBalanceRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletBalanceResponse>> Handle(GetWalletBalanceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var balance = await _walletBalanceRepository.GetByIdAsync(request.Id, cancellationToken);
            var response = _mapper.Map<WalletBalanceResponse>(balance);
            return Result.Success(response);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletBalanceResponse>(new Error("WalletBalance.NotFound", $"Wallet balance with id {request.Id} was not found."));
        }
    }
}
