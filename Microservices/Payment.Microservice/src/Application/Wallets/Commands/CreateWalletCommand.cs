using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Wallets.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Wallets.Commands;

public sealed record CreateWalletCommand(
    Guid UserId,
    bool IsSystem,
    WalletStatus Status = WalletStatus.Active
) : ICommand<WalletResponse>;

internal sealed class CreateWalletCommandHandler : ICommandHandler<CreateWalletCommand, WalletResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public CreateWalletCommandHandler(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletResponse>> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = _mapper.Map<Wallet>(request);
        var now = DateTime.UtcNow;

        wallet.Id = Guid.NewGuid();
        wallet.CreatedAt = now;
        wallet.UpdatedAt = now;

        await _walletRepository.AddAsync(wallet, cancellationToken);

        var response = _mapper.Map<WalletResponse>(wallet);
        return Result.Success(response);
    }
}
