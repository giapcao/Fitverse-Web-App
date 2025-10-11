using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Wallets.Queries;

public sealed record GetWalletByIdQuery(Guid Id) : IQuery<WalletResponse>;

internal sealed class GetWalletByIdQueryHandler : IQueryHandler<GetWalletByIdQuery, WalletResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public GetWalletByIdQueryHandler(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletResponse>> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await _walletRepository.GetByIdAsync(request.Id, cancellationToken);
            var response = _mapper.Map<WalletResponse>(wallet);
            return Result.Success(response);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletResponse>(new Error("Wallet.NotFound", $"Wallet with id {request.Id} was not found."));
        }
    }
}
