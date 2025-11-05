using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletBalances.Queries;

public sealed record GetWalletBalancesByUserIdQuery(Guid UserId) : IQuery<IEnumerable<WalletBalanceResponse>>;

internal sealed class GetWalletBalancesByUserIdQueryHandler
    : IQueryHandler<GetWalletBalancesByUserIdQuery, IEnumerable<WalletBalanceResponse>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IMapper _mapper;

    public GetWalletBalancesByUserIdQueryHandler(
        IWalletRepository walletRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _walletBalanceRepository = walletBalanceRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WalletBalanceResponse>>> Handle(
        GetWalletBalancesByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.FindAsync(
            wallet => wallet.UserId == request.UserId,
            cancellationToken);

        var wallet = wallets.FirstOrDefault();
        if (wallet is null)
        {
            return Result.Failure<IEnumerable<WalletBalanceResponse>>(
                new Error("Wallet.NotFound", $"Wallet for user id {request.UserId} was not found."));
        }

        var balances = await _walletBalanceRepository.FindAsync(
            balance => balance.WalletId == wallet.Id,
            cancellationToken);

        var response = _mapper.Map<IEnumerable<WalletBalanceResponse>>(balances);
        return Result.Success(response);
    }
}
