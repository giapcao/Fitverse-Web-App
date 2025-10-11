using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Wallets.Queries;

public sealed record GetWalletsQuery : IQuery<IEnumerable<WalletResponse>>;

internal sealed class GetWalletsQueryHandler : IQueryHandler<GetWalletsQuery, IEnumerable<WalletResponse>>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public GetWalletsQueryHandler(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WalletResponse>>> Handle(GetWalletsQuery request, CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.GetAllAsync(cancellationToken);
        var response = _mapper.Map<IEnumerable<WalletResponse>>(wallets);
        return Result.Success(response);
    }
}
