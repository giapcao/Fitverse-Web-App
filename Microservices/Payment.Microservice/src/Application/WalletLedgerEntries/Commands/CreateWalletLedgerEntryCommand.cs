using System;
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

public sealed record CreateWalletLedgerEntryCommand(
    Guid JournalId,
    Guid WalletId,
    long AmountVnd,
    string? Description,
    Dc Dc = Dc.Debit
) : ICommand<WalletLedgerEntryResponse>;

internal sealed class CreateWalletLedgerEntryCommandHandler : ICommandHandler<CreateWalletLedgerEntryCommand, WalletLedgerEntryResponse>
{
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IMapper _mapper;

    public CreateWalletLedgerEntryCommandHandler(
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IMapper mapper)
    {
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletLedgerEntryResponse>> Handle(CreateWalletLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = _mapper.Map<WalletLedgerEntry>(request);
        entry.Id = Guid.NewGuid();
        entry.CreatedAt = DateTime.UtcNow;

        await _walletLedgerEntryRepository.AddAsync(entry, cancellationToken);

        var response = _mapper.Map<WalletLedgerEntryResponse>(entry);
        return Result.Success(response);
    }
}
