using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public sealed record CaptureBookingWalletPaymentCommand(
    Guid WalletId,
    Guid UserId,
    Guid WalletJournalId,
    long AmountVnd) : ICommand;

internal sealed class CaptureBookingWalletPaymentCommandHandler
    : ICommandHandler<CaptureBookingWalletPaymentCommand>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly ILogger<CaptureBookingWalletPaymentCommandHandler> _logger;

    public CaptureBookingWalletPaymentCommandHandler(
        IWalletRepository walletRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        ILogger<CaptureBookingWalletPaymentCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _walletBalanceRepository = walletBalanceRepository;
        _walletJournalRepository = walletJournalRepository;
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(CaptureBookingWalletPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.AmountVnd <= 0)
        {
            return Result.Failure(new Error("Payment.AmountInvalid", "Amount must be greater than zero."));
        }

        var wallet = await FindWalletAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure(new Error("Wallet.NotFound", $"Wallet with id {request.WalletId} was not found."));
        }

        if (wallet.UserId != request.UserId)
        {
            return Result.Failure(new Error("Wallet.UserMismatch", "Wallet does not belong to the supplied user."));
        }

        var holdJournal = await FindHoldJournalAsync(request.WalletJournalId, cancellationToken);
        if (holdJournal is null)
        {
            return Result.Failure(new Error(
                "WalletJournal.HoldMissing",
                "Hold journal for the booking could not be located."));
        }

        if (holdJournal.Status == WalletJournalStatus.Posted)
        {
            _logger.LogInformation(
                "Booking wallet capture requested for already captured journal {WalletJournalId}.",
                holdJournal.Id);
            return Result.Success();
        }

        var balances = await _walletBalanceRepository.FindAsync(
            balance => balance.WalletId == request.WalletId,
            cancellationToken);

        var walletBalances = balances as WalletBalance[] ?? balances.ToArray();
        var availableBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Available);
        if (availableBalance is null)
        {
            return Result.Failure(new Error(
                "WalletBalance.AvailableMissing",
                "Available balance could not be found for the wallet."));
        }

        if (availableBalance.BalanceVnd < request.AmountVnd)
        {
            return Result.Failure(new Error(
                "Wallet.InsufficientFunds",
                "Wallet does not have sufficient available balance to complete the payment."));
        }

        var now = DateTime.UtcNow;
        var escrowBalance = walletBalances.FirstOrDefault(balance => balance.AccountType == WalletAccountType.Escrow);

        availableBalance.BalanceVnd -= request.AmountVnd;
        availableBalance.UpdatedAt = now;
        _walletBalanceRepository.Update(availableBalance);

        if (escrowBalance is null)
        {
            escrowBalance = new WalletBalance
            {
                Id = Guid.NewGuid(),
                WalletId = request.WalletId,
                BalanceVnd = request.AmountVnd,
                AccountType = WalletAccountType.Escrow,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _walletBalanceRepository.AddAsync(escrowBalance, cancellationToken);
        }
        else
        {
            escrowBalance.BalanceVnd += request.AmountVnd;
            escrowBalance.UpdatedAt = now;
            _walletBalanceRepository.Update(escrowBalance);
        }

        wallet.UpdatedAt = now;
        _walletRepository.Update(wallet);

        holdJournal.Status = WalletJournalStatus.Posted;
        holdJournal.PostedAt = now;
        _walletJournalRepository.Update(holdJournal);

        var payoutJournal = new WalletJournal
        {
            Id = Guid.NewGuid(),
            BookingId = holdJournal.BookingId,
            PaymentId = holdJournal.PaymentId,
            Status = WalletJournalStatus.Posted,
            Type = WalletJournalType.Payout,
            CreatedAt = now,
            PostedAt = now
        };

        await _walletJournalRepository.AddAsync(payoutJournal, cancellationToken);

        await CreateOrUpdateLedgerEntryAsync(
            holdJournal,
            request.WalletId,
            request.AmountVnd,
            now,
            WalletAccountType.Escrow,
            Dc.Debit,
            $"Booking wallet hold capture for journal {holdJournal.Id}",
            cancellationToken);

        await CreateOrUpdateLedgerEntryAsync(
            payoutJournal,
            request.WalletId,
            request.AmountVnd,
            now,
            WalletAccountType.Available,
            Dc.Credit,
            $"Booking wallet payout for journal {holdJournal.Id}",
            cancellationToken);

        _logger.LogInformation(
            "Booking wallet journal {WalletJournalId} captured for wallet {WalletId}.",
            holdJournal.Id,
            request.WalletId);

        return Result.Success();
    }

    private async Task<Wallet?> FindWalletAsync(Guid walletId, CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.FindAsync(wallet => wallet.Id == walletId, cancellationToken);
        return wallets.FirstOrDefault();
    }

    private async Task<WalletJournal?> FindHoldJournalAsync(Guid walletJournalId, CancellationToken cancellationToken)
    {
        var journals = await _walletJournalRepository.FindAsync(
            journal => journal.Id == walletJournalId && journal.Type == WalletJournalType.Hold,
            cancellationToken);

        return journals.FirstOrDefault();
    }

    private async Task CreateOrUpdateLedgerEntryAsync(
        WalletJournal journal,
        Guid walletId,
        long amountVnd,
        DateTime timestamp,
        WalletAccountType accountType,
        Dc dc,
        string description,
        CancellationToken cancellationToken)
    {
        var entries = await _walletLedgerEntryRepository.FindAsync(
            entry => entry.JournalId == journal.Id,
            cancellationToken);

        var entry = entries.FirstOrDefault();
        if (entry is null)
        {
            entry = new WalletLedgerEntry
            {
                Id = Guid.NewGuid(),
                JournalId = journal.Id,
                WalletId = walletId,
                AmountVnd = amountVnd,
                Dc = dc,
                AccountType = accountType,
                Description = description,
                CreatedAt = timestamp
            };

            await _walletLedgerEntryRepository.AddAsync(entry, cancellationToken);
        }
        else
        {
            entry.WalletId = walletId;
            entry.AmountVnd = amountVnd;
            entry.Dc = dc;
            entry.AccountType = accountType;
            entry.Description = description;
            _walletLedgerEntryRepository.Update(entry);
        }
    }
}
