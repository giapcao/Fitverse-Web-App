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
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;

namespace Application.WithdrawalRequests.Commands;

public sealed record UpdateWithdrawalRequestStatusCommand(
    Guid Id,
    WithdrawalRequestStatus Status,
    string? RejectionReason) : ICommand<WithdrawalRequestResponse>;

internal sealed class UpdateWithdrawalRequestStatusCommandHandler
    : ICommandHandler<UpdateWithdrawalRequestStatusCommand, WithdrawalRequestResponse>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateWithdrawalRequestStatusCommandHandler> _logger;

    public UpdateWithdrawalRequestStatusCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletRepository walletRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IMapper mapper,
        ILogger<UpdateWithdrawalRequestStatusCommandHandler> logger)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletRepository = walletRepository;
        _walletBalanceRepository = walletBalanceRepository;
        _walletJournalRepository = walletJournalRepository;
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WithdrawalRequestResponse>> Handle(
        UpdateWithdrawalRequestStatusCommand request,
        CancellationToken cancellationToken)
    {
        WithdrawalRequest withdrawalRequest;

        try
        {
            withdrawalRequest = await _withdrawalRequestRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "WithdrawalRequest.NotFound",
                $"Withdrawal request with id {request.Id} was not found."));
        }

        if (!IsTransitionAllowed(withdrawalRequest.Status, request.Status))
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "WithdrawalRequest.InvalidStatusTransition",
                $"Cannot transition withdrawal request from {withdrawalRequest.Status} to {request.Status}."));
        }

        var now = DateTime.UtcNow;

        if (withdrawalRequest.Status == request.Status)
        {
            if (request.Status == WithdrawalRequestStatus.Rejected &&
                !string.IsNullOrWhiteSpace(request.RejectionReason) &&
                !string.Equals(withdrawalRequest.RejectionReason, request.RejectionReason, StringComparison.Ordinal))
            {
                withdrawalRequest.RejectionReason = request.RejectionReason;
                withdrawalRequest.RejectedAt ??= now;
                withdrawalRequest.UpdatedAt = now;
                _withdrawalRequestRepository.Update(withdrawalRequest);
            }

            var unchangedResponse = _mapper.Map<WithdrawalRequestResponse>(withdrawalRequest);
            return Result.Success(unchangedResponse);
        }

        Result statusResult = request.Status switch
        {
            WithdrawalRequestStatus.Pending => Result.Failure(new Error(
                "WithdrawalRequest.PendingNotSupported",
                "Reverting withdrawal requests back to pending is not supported.")),
            WithdrawalRequestStatus.Approved => await ApproveAsync(withdrawalRequest, now, cancellationToken),
            WithdrawalRequestStatus.Completed => await CompleteAsync(withdrawalRequest, now, cancellationToken),
            WithdrawalRequestStatus.Rejected => await RejectAsync(withdrawalRequest, request.RejectionReason, now, cancellationToken),
            _ => Result.Failure(new Error(
                "WithdrawalRequest.StatusUnknown",
                $"Status '{request.Status}' is not recognized."))
        };

        if (statusResult.IsFailure)
        {
            return Result.Failure<WithdrawalRequestResponse>(statusResult.Error);
        }

        withdrawalRequest.Status = request.Status;
        withdrawalRequest.UpdatedAt = now;

        _withdrawalRequestRepository.Update(withdrawalRequest);

        var response = _mapper.Map<WithdrawalRequestResponse>(withdrawalRequest);
        return Result.Success(response);
    }

    private Task<Result> ApproveAsync(
        WithdrawalRequest withdrawalRequest,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        if (withdrawalRequest.Status == WithdrawalRequestStatus.Completed ||
            withdrawalRequest.Status == WithdrawalRequestStatus.Rejected)
        {
            return Task.FromResult(Result.Failure(new Error(
                "WithdrawalRequest.StatusLocked",
                "Approved status cannot be applied to completed or rejected requests.")));
        }

        withdrawalRequest.ApprovedAt = timestamp;
        withdrawalRequest.CompletedAt = null;
        withdrawalRequest.RejectedAt = null;
        withdrawalRequest.RejectionReason = null;

        _logger.LogInformation(
            "Withdrawal request {WithdrawalRequestId} approved at {Timestamp}.",
            withdrawalRequest.Id,
            timestamp);

        return Task.FromResult(Result.Success());
    }

    private async Task<Result> CompleteAsync(
        WithdrawalRequest withdrawalRequest,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        if (withdrawalRequest.Status == WithdrawalRequestStatus.Rejected)
        {
            return Result.Failure(new Error(
                "WithdrawalRequest.AlreadyRejected",
                "Cannot complete a withdrawal request that has been rejected."));
        }

        Wallet wallet;
        try
        {
            wallet = await _walletRepository.GetByIdAsync(withdrawalRequest.WalletId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error(
                "Wallet.NotFound",
                $"Wallet with id {withdrawalRequest.WalletId} was not found."));
        }

        var balances = await _walletBalanceRepository.FindAsync(
            balance => balance.WalletId == withdrawalRequest.WalletId,
            cancellationToken);

        var walletBalances = balances as WalletBalance[] ?? balances.ToArray();
        var frozenBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Frozen);

        if (frozenBalance is null || frozenBalance.BalanceVnd < withdrawalRequest.AmountVnd)
        {
            return Result.Failure(new Error(
                "WalletBalance.FrozenInsufficient",
                "Frozen balance is insufficient to complete the withdrawal."));
        }

        frozenBalance.BalanceVnd -= withdrawalRequest.AmountVnd;
        frozenBalance.UpdatedAt = timestamp;
        _walletBalanceRepository.Update(frozenBalance);

        wallet.UpdatedAt = timestamp;
        _walletRepository.Update(wallet);

        var payoutJournal = new WalletJournal
        {
            Id = Guid.NewGuid(),
            Status = WalletJournalStatus.Posted,
            Type = WalletJournalType.Payout,
            CreatedAt = timestamp,
            PostedAt = timestamp
        };

        await _walletJournalRepository.AddAsync(payoutJournal, cancellationToken);

        var payoutLedgerEntry = new WalletLedgerEntry
        {
            Id = Guid.NewGuid(),
            JournalId = payoutJournal.Id,
            WalletId = withdrawalRequest.WalletId,
            AmountVnd = withdrawalRequest.AmountVnd,
            Dc = Dc.Credit,
            AccountType = WalletAccountType.Frozen,
            Description = $"Withdrawal request {withdrawalRequest.Id} completed.",
            CreatedAt = timestamp
        };

        await _walletLedgerEntryRepository.AddAsync(payoutLedgerEntry, cancellationToken);

        withdrawalRequest.ApprovedAt ??= timestamp;
        withdrawalRequest.CompletedAt = timestamp;
        withdrawalRequest.RejectedAt = null;
        withdrawalRequest.RejectionReason = null;
        withdrawalRequest.PayoutWalletJournalId = payoutJournal.Id;

        _logger.LogInformation(
            "Withdrawal request {WithdrawalRequestId} completed and payout journal {WalletJournalId} created.",
            withdrawalRequest.Id,
            payoutJournal.Id);

        return Result.Success();
    }

    private async Task<Result> RejectAsync(
        WithdrawalRequest withdrawalRequest,
        string? rejectionReason,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            return Result.Failure(new Error(
                "WithdrawalRequest.RejectionReasonRequired",
                "Rejection reason is required when rejecting a withdrawal request."));
        }

        Wallet wallet;
        try
        {
            wallet = await _walletRepository.GetByIdAsync(withdrawalRequest.WalletId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error(
                "Wallet.NotFound",
                $"Wallet with id {withdrawalRequest.WalletId} was not found."));
        }

        var balances = await _walletBalanceRepository.FindAsync(
            balance => balance.WalletId == withdrawalRequest.WalletId,
            cancellationToken);

        var walletBalances = balances as WalletBalance[] ?? balances.ToArray();
        var frozenBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Frozen);
        var availableBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Available);

        if (availableBalance is null)
        {
            return Result.Failure(new Error(
                "WalletBalance.AvailableMissing",
                "Available balance could not be found for the wallet."));
        }

        if (frozenBalance is null || frozenBalance.BalanceVnd < withdrawalRequest.AmountVnd)
        {
            return Result.Failure(new Error(
                "WalletBalance.FrozenInsufficient",
                "Frozen balance is insufficient to reject the withdrawal request."));
        }

        frozenBalance.BalanceVnd -= withdrawalRequest.AmountVnd;
        frozenBalance.UpdatedAt = timestamp;
        _walletBalanceRepository.Update(frozenBalance);

        availableBalance.BalanceVnd += withdrawalRequest.AmountVnd;
        availableBalance.UpdatedAt = timestamp;
        _walletBalanceRepository.Update(availableBalance);

        wallet.UpdatedAt = timestamp;
        _walletRepository.Update(wallet);

        var releaseJournal = new WalletJournal
        {
            Id = Guid.NewGuid(),
            Status = WalletJournalStatus.Posted,
            Type = WalletJournalType.Release,
            CreatedAt = timestamp,
            PostedAt = timestamp
        };

        await _walletJournalRepository.AddAsync(releaseJournal, cancellationToken);

        var frozenLedgerEntry = new WalletLedgerEntry
        {
            Id = Guid.NewGuid(),
            JournalId = releaseJournal.Id,
            WalletId = withdrawalRequest.WalletId,
            AmountVnd = withdrawalRequest.AmountVnd,
            Dc = Dc.Credit,
            AccountType = WalletAccountType.Frozen,
            Description = $"Withdrawal request {withdrawalRequest.Id} rejected - frozen funds released.",
            CreatedAt = timestamp
        };

        var availableLedgerEntry = new WalletLedgerEntry
        {
            Id = Guid.NewGuid(),
            JournalId = releaseJournal.Id,
            WalletId = withdrawalRequest.WalletId,
            AmountVnd = withdrawalRequest.AmountVnd,
            Dc = Dc.Debit,
            AccountType = WalletAccountType.Available,
            Description = $"Withdrawal request {withdrawalRequest.Id} rejected - available balance restored.",
            CreatedAt = timestamp
        };

        await _walletLedgerEntryRepository.AddAsync(frozenLedgerEntry, cancellationToken);
        await _walletLedgerEntryRepository.AddAsync(availableLedgerEntry, cancellationToken);

        withdrawalRequest.RejectedAt = timestamp;
        withdrawalRequest.RejectionReason = rejectionReason;
        withdrawalRequest.CompletedAt = null;
        withdrawalRequest.PayoutWalletJournalId = null;

        _logger.LogInformation(
            "Withdrawal request {WithdrawalRequestId} rejected and funds returned to available balance.",
            withdrawalRequest.Id);

        return Result.Success();
    }

    private static bool IsTransitionAllowed(WithdrawalRequestStatus current, WithdrawalRequestStatus next)
    {
        if (current == next)
        {
            return true;
        }

        return current switch
        {
            WithdrawalRequestStatus.Pending => next is WithdrawalRequestStatus.Approved
                or WithdrawalRequestStatus.Completed
                or WithdrawalRequestStatus.Rejected,
            WithdrawalRequestStatus.Approved => next is WithdrawalRequestStatus.Completed
                or WithdrawalRequestStatus.Rejected,
            WithdrawalRequestStatus.Completed => next == WithdrawalRequestStatus.Completed,
            WithdrawalRequestStatus.Rejected => next == WithdrawalRequestStatus.Rejected,
            _ => false
        };
    }
}
