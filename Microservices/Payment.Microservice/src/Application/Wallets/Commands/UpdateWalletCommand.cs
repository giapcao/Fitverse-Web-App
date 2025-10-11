using System;
using System.Collections.Generic;
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

public sealed record UpdateWalletCommand(
    Guid Id,
    Guid UserId,
    bool IsSystem,
    WalletStatus Status
) : ICommand<WalletResponse>;

internal sealed class UpdateWalletCommandHandler : ICommandHandler<UpdateWalletCommand, WalletResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public UpdateWalletCommandHandler(IWalletRepository walletRepository, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletResponse>> Handle(UpdateWalletCommand request, CancellationToken cancellationToken)
    {
        Wallet wallet;

        try
        {
            wallet = await _walletRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletResponse>(new Error("Wallet.NotFound", $"Wallet with id {request.Id} was not found."));
        }

        _mapper.Map(request, wallet);
        wallet.UpdatedAt = DateTime.UtcNow;

        _walletRepository.Update(wallet);

        var response = _mapper.Map<WalletResponse>(wallet);
        return Result.Success(response);
    }
}
