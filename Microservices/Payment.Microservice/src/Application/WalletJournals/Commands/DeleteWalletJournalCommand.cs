using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletJournals.Commands;

public sealed record DeleteWalletJournalCommand(Guid Id) : ICommand;

internal sealed class DeleteWalletJournalCommandHandler : ICommandHandler<DeleteWalletJournalCommand>
{
    private readonly IWalletJournalRepository _walletJournalRepository;

    public DeleteWalletJournalCommandHandler(IWalletJournalRepository walletJournalRepository)
    {
        _walletJournalRepository = walletJournalRepository;
    }

    public async Task<Result> Handle(DeleteWalletJournalCommand request, CancellationToken cancellationToken)
    {
        WalletJournal journal;

        try
        {
            journal = await _walletJournalRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error("WalletJournal.NotFound", $"Wallet journal with id {request.Id} was not found."));
        }

        _walletJournalRepository.Delete(journal);

        return Result.Success();
    }
}
