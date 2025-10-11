using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.WalletLedgerEntries.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletLedgerEntries.Commands;

public sealed record UpdateWalletLedgerEntryCommand(
    Guid Id,
    Guid JournalId,
    Guid WalletId,
    long AmountVnd,
    string? Description,
    Dc Dc
) : ICommand<WalletLedgerEntryResponse>;

internal sealed class UpdateWalletLedgerEntryCommandHandler : ICommandHandler<UpdateWalletLedgerEntryCommand, WalletLedgerEntryResponse>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IMapper _mapper;

    public UpdateWalletLedgerEntryCommandHandler(
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IMapper mapper)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletLedgerEntryResponse>> Handle(UpdateWalletLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        WalletLedgerEntry entry;

        try
        {
            entry = await _walletLedgerEntryRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletLedgerEntryResponse>(new Error("WalletLedgerEntry.NotFound", $"Wallet ledger entry with id {request.Id} was not found."));
        }

        _mapper.Map(request, entry);

        _walletLedgerEntryRepository.Update(entry);

        var response = _mapper.Map<WalletLedgerEntryResponse>(entry);
        return Result.Success(response);
    }
}
