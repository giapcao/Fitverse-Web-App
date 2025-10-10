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

namespace Application.WalletBalances.Queries;

public sealed record GetWalletBalancesQuery(Guid? WalletId) : IQuery<IEnumerable<WalletBalanceResponse>>;

internal sealed class GetWalletBalancesQueryHandler : IQueryHandler<GetWalletBalancesQuery, IEnumerable<WalletBalanceResponse>>
{
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IMapper _mapper;

    public GetWalletBalancesQueryHandler(IWalletBalanceRepository walletBalanceRepository, IMapper mapper)
    {
        _walletBalanceRepository = walletBalanceRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WalletBalanceResponse>>> Handle(GetWalletBalancesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<WalletBalance> balances;

        if (request.WalletId.HasValue)
        {
            Expression<Func<WalletBalance, bool>> predicate = balance => balance.WalletId == request.WalletId.Value;
            balances = await _walletBalanceRepository.FindAsync(predicate, cancellationToken);
        }
        else
        {
            balances = await _walletBalanceRepository.GetAllAsync(cancellationToken);
        }

        var response = _mapper.Map<IEnumerable<WalletBalanceResponse>>(balances);
        return Result.Success(response);
    }
}
