using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.WithdrawalRequests.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WithdrawalRequests.Commands;

public sealed record CreateWithdrawalRequestCommand(
    Guid WalletId,
    Guid UserId,
    long AmountVnd) : ICommand<WithdrawalRequestResponse>;

internal sealed class CreateWithdrawalRequestCommandHandler
    : ICommandHandler<CreateWithdrawalRequestCommand, WithdrawalRequestResponse>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IMapper _mapper;

    public CreateWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletRepository walletRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IMapper mapper)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletRepository = walletRepository;
        _walletBalanceRepository = walletBalanceRepository;
        _walletJournalRepository = walletJournalRepository;
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _mapper = mapper;
    }

    public async Task<Result<WithdrawalRequestResponse>> Handle(
        CreateWithdrawalRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.AmountVnd <= 0)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "WithdrawalRequest.AmountInvalid",
                "Amount must be greater than zero."));
        }

        Wallet wallet;
        try
        {
            wallet = await _walletRepository.GetByIdAsync(request.WalletId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "Wallet.NotFound",
                $"Wallet with id {request.WalletId} was not found."));
        }

        if (wallet.UserId != request.UserId)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "WithdrawalRequest.UserMismatch",
                "Wallet does not belong to the supplied user."));
        }

        var balances = await _walletBalanceRepository.FindAsync(
            balance => balance.WalletId == request.WalletId,
            cancellationToken);

        var walletBalances = balances as WalletBalance[] ?? balances.ToArray();
        var availableBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Available);
        if (availableBalance is null)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "WalletBalance.AvailableMissing",
                "Available balance could not be found for the wallet."));
        }

        if (availableBalance.BalanceVnd < request.AmountVnd)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "Wallet.InsufficientFunds",
                "Wallet does not have sufficient available balance to fulfill the withdrawal request."));
        }

        var now = DateTime.UtcNow;

        availableBalance.BalanceVnd -= request.AmountVnd;
        availableBalance.UpdatedAt = now;
        _walletBalanceRepository.Update(availableBalance);

        var frozenBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Frozen);
        if (frozenBalance is null)
        {
            frozenBalance = new WalletBalance
            {
                Id = Guid.NewGuid(),
                WalletId = request.WalletId,
                BalanceVnd = request.AmountVnd,
                AccountType = WalletAccountType.Frozen,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _walletBalanceRepository.AddAsync(frozenBalance, cancellationToken);
        }
        else
        {
            frozenBalance.BalanceVnd += request.AmountVnd;
            frozenBalance.UpdatedAt = now;
            _walletBalanceRepository.Update(frozenBalance);
        }

        wallet.UpdatedAt = now;
        _walletRepository.Update(wallet);

        var holdJournal = new WalletJournal
        {
            Id = Guid.NewGuid(),
            Status = WalletJournalStatus.Pending,
            Type = WalletJournalType.WithdrawalHold,
            CreatedAt = now,
            PostedAt = now
        };

        await _walletJournalRepository.AddAsync(holdJournal, cancellationToken);

        var holdLedgerEntry = new WalletLedgerEntry
        {
            Id = Guid.NewGuid(),
            JournalId = holdJournal.Id,
            WalletId = request.WalletId,
            AmountVnd = request.AmountVnd,
            Dc = Dc.Debit,
            AccountType = WalletAccountType.Frozen,
            Description = $"Withdrawal hold for wallet {request.WalletId}",
            CreatedAt = now
        };

        await _walletLedgerEntryRepository.AddAsync(holdLedgerEntry, cancellationToken);

        var withdrawalRequest = new WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            WalletId = request.WalletId,
            UserId = request.UserId,
            AmountVnd = request.AmountVnd,
            Status = WithdrawalRequestStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
            HoldWalletJournalId = holdJournal.Id
        };

        await _withdrawalRequestRepository.AddAsync(withdrawalRequest, cancellationToken);

        var response = _mapper.Map<WithdrawalRequestResponse>(withdrawalRequest);
        return Result.Success(response);
    }
}
