using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.WalletJournals.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletJournals.Commands;

public sealed record CreateWalletJournalCommand(
    Guid? BookingId,
    Guid? PaymentId,
    DateTime? PostedAt,
    WalletJournalStatus Status = WalletJournalStatus.Pending,
    WalletJournalType JournalType = WalletJournalType.Deposit
) : ICommand<WalletJournalResponse>;

internal sealed class CreateWalletJournalCommandHandler : ICommandHandler<CreateWalletJournalCommand, WalletJournalResponse>
{
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IMapper _mapper;

    public CreateWalletJournalCommandHandler(IWalletJournalRepository walletJournalRepository, IMapper mapper)
    {
        _walletJournalRepository = walletJournalRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletJournalResponse>> Handle(CreateWalletJournalCommand request, CancellationToken cancellationToken)
    {
        var journal = _mapper.Map<WalletJournal>(request);
        var now = DateTime.UtcNow;

        journal.Id = Guid.NewGuid();
        journal.CreatedAt = now;
        journal.PostedAt ??= now;

        await _walletJournalRepository.AddAsync(journal, cancellationToken);

        var response = _mapper.Map<WalletJournalResponse>(journal);
        return Result.Success(response);
    }
}
