using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletBalances.Commands;

public sealed record DeleteWalletBalanceCommand(Guid Id) : ICommand;

internal sealed class DeleteWalletBalanceCommandHandler : ICommandHandler<DeleteWalletBalanceCommand>
{
    private readonly IWalletBalanceRepository _walletBalanceRepository;

    public DeleteWalletBalanceCommandHandler(IWalletBalanceRepository walletBalanceRepository)
    {
        _walletBalanceRepository = walletBalanceRepository;
    }

    public async Task<Result> Handle(DeleteWalletBalanceCommand request, CancellationToken cancellationToken)
    {
        WalletBalance balance;

        try
        {
            balance = await _walletBalanceRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error("WalletBalance.NotFound", $"Wallet balance with id {request.Id} was not found."));
        }

        _walletBalanceRepository.Delete(balance);

        return Result.Success();
    }
}
