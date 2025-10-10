using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletLedgerEntries.Commands;

public sealed record DeleteWalletLedgerEntryCommand(Guid Id) : ICommand;

internal sealed class DeleteWalletLedgerEntryCommandHandler : ICommandHandler<DeleteWalletLedgerEntryCommand>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;

    public DeleteWalletLedgerEntryCommandHandler(IWalletLedgerEntryRepository walletLedgerEntryRepository)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
    }

    public async Task<Result> Handle(DeleteWalletLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        WalletLedgerEntry entry;

        try
        {
            entry = await _walletLedgerEntryRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error("WalletLedgerEntry.NotFound", $"Wallet ledger entry with id {request.Id} was not found."));
        }

        _walletLedgerEntryRepository.Delete(entry);

        return Result.Success();
    }
}
