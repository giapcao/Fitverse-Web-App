using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Wallets.Commands;

public sealed record DeleteWalletCommand(Guid Id) : ICommand;

internal sealed class DeleteWalletCommandHandler : ICommandHandler<DeleteWalletCommand>
{
    private readonly IWalletRepository _walletRepository;

    public DeleteWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(DeleteWalletCommand request, CancellationToken cancellationToken)
    {
        Wallet wallet;

        try
        {
            wallet = await _walletRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error("Wallet.NotFound", $"Wallet with id {request.Id} was not found."));
        }

        _walletRepository.Delete(wallet);

        return Result.Success();
    }
}
