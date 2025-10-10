using System;
using System.Collections.Generic;
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

public sealed record UpdateWalletJournalCommand(
    Guid Id,
    Guid? BookingId,
    Guid? PaymentId,
    DateTime? PostedAt,
    WalletJournalStatus Status,
    WalletJournalType JournalType
) : ICommand<WalletJournalResponse>;

internal sealed class UpdateWalletJournalCommandHandler : ICommandHandler<UpdateWalletJournalCommand, WalletJournalResponse>
{
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IMapper _mapper;

    public UpdateWalletJournalCommandHandler(IWalletJournalRepository walletJournalRepository, IMapper mapper)
    {
        _walletJournalRepository = walletJournalRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletJournalResponse>> Handle(UpdateWalletJournalCommand request, CancellationToken cancellationToken)
    {
        WalletJournal journal;

        try
        {
            journal = await _walletJournalRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletJournalResponse>(new Error("WalletJournal.NotFound", $"Wallet journal with id {request.Id} was not found."));
        }

        _mapper.Map(request, journal);

        _walletJournalRepository.Update(journal);

        var response = _mapper.Map<WalletJournalResponse>(journal);
        return Result.Success(response);
    }
}
