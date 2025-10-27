using Application.Payments.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Returns;

internal abstract class PaymentGatewayReturnHandlerBase<TConfig> : IPaymentGatewayReturnHandler
    where TConfig : IPaymentGatewayConfiguration
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;
    private readonly IWalletLedgerEntryRepository _walletLedgerEntryRepository;
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    protected PaymentGatewayReturnHandlerBase(
        IPaymentRepository paymentRepository,
        IWalletJournalRepository walletJournalRepository,
        IWalletLedgerEntryRepository walletLedgerEntryRepository,
        IWalletBalanceRepository walletBalanceRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _paymentRepository = paymentRepository;
        _walletJournalRepository = walletJournalRepository;
        _walletLedgerEntryRepository = walletLedgerEntryRepository;
        _walletBalanceRepository = walletBalanceRepository;
        _walletRepository = walletRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public abstract Gateway Gateway { get; }

    public async Task<Result<PaymentGatewayReturnResult>> HandleAsync(PaymentGatewayReturnContext context,
        CancellationToken cancellationToken)
    {
        if (context.Configuration is not TConfig config)
        {
            return Result.Failure<PaymentGatewayReturnResult>(PaymentReturnErrors.InvalidConfiguration(Gateway));
        }

        if (!IsConfigurationValid(config))
        {
            return Result.Failure<PaymentGatewayReturnResult>(GetConfigurationError());
        }

        var parameters = new Dictionary<string, string>(context.QueryParameters, StringComparer.OrdinalIgnoreCase);
        var userId = ResolveUserId(parameters, context.UserId);
        var signatureValid = ValidateSignature(parameters, config);

        if (!signatureValid)
        {
            _logger.LogWarning("{Gateway} return signature invalid for payload {@Payload}", Gateway, parameters);
            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        userId = ResolveUserId(parameters, userId);

        if (!TryResolvePaymentId(parameters, out var paymentId))
        {
            _logger.LogWarning("{Gateway} return could not resolve payment id for payload {@Payload}", Gateway,
                parameters);
            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        var payments = await _paymentRepository.FindAsync(payment => payment.Id == paymentId, cancellationToken);
        var payment = payments.FirstOrDefault();
        if (payment is null)
        {
            _logger.LogWarning("{Gateway} return received for missing payment {PaymentId}", Gateway, paymentId);
            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        if (!TryResolveAmount(parameters, out var normalizedAmount))
        {
            _logger.LogWarning("{Gateway} return amount invalid for payment {PaymentId}", Gateway, payment.Id);
            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        if (payment.AmountVnd != normalizedAmount)
        {
            _logger.LogWarning(
                "{Gateway} return amount mismatch for payment {PaymentId}. Expected {Expected}, actual {Actual}",
                Gateway, payment.Id, payment.AmountVnd, normalizedAmount);
            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        var responseCode = ResolveResponseCode(parameters);
        var transactionStatus = ResolveTransactionStatus(parameters);

        if (payment.Status == PaymentStatus.Captured)
        {
            _logger.LogInformation("{Gateway} return acknowledged already captured payment {PaymentId}", Gateway,
                payment.Id);
            var existingWallet = await ResolveWalletFromLedgerAsync(payment.Id, cancellationToken);
            if (existingWallet is not null)
            {
                return Result.Success(new PaymentGatewayReturnResult(true, existingWallet.UserId));
            }

            return Result.Success(new PaymentGatewayReturnResult(true, userId));
        }

        if (!IsSuccess(responseCode, transactionStatus))
        {
            _logger.LogWarning(
                "{Gateway} return indicates failure for payment {PaymentId} with responseCode {ResponseCode} and transactionStatus {TransactionStatus}",
                Gateway, payment.Id, responseCode, transactionStatus);
            MarkPaymentAsFailed(payment);
            _paymentRepository.Update(payment);

            await UpdateJournalsAsync(payment.Id, WalletJournalStatus.Cancelled, null, cancellationToken);

            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        var journal = await ResolveJournalAsync(payment.Id, cancellationToken);
        if (journal is null)
        {
            _logger.LogWarning("{Gateway} return could not locate wallet journal for payment {PaymentId}", Gateway,
                payment.Id);
            return Result.Success(new PaymentGatewayReturnResult(false, userId));
        }

        var walletResolution = await ResolveWalletAsync(userId, payment.Id, cancellationToken);
        if (!walletResolution.HasUserId)
        {
            return Result.Success(new PaymentGatewayReturnResult(false, null));
        }

        var walletId = walletResolution.WalletId ?? Guid.NewGuid();
        userId = walletResolution.UserId;

        var now = DateTime.UtcNow;

        payment.Status = PaymentStatus.Captured;
        payment.Gateway = Gateway;
        payment.PaidAt = now;

        var gatewayTransactionId = ResolveGatewayTransactionId(parameters);
        if (!string.IsNullOrWhiteSpace(gatewayTransactionId))
        {
            payment.GatewayTxnId = gatewayTransactionId;
        }

        _paymentRepository.Update(payment);

        journal.Status = WalletJournalStatus.Posted;
        journal.PostedAt = now;
        _walletJournalRepository.Update(journal);

        var wallet = await EnsureWalletAsync(walletId, userId!.Value, now, cancellationToken);
        await EnsureWalletBalanceAsync(journal, wallet.Id, normalizedAmount, now, cancellationToken);
        await EnsureLedgerEntryAsync(journal, wallet.Id, normalizedAmount, now, cancellationToken);

        return Result.Success(new PaymentGatewayReturnResult(true, userId));
    }

    protected abstract bool IsConfigurationValid(TConfig configuration);

    protected abstract Error GetConfigurationError();

    protected abstract bool ValidateSignature(
        IReadOnlyDictionary<string, string> parameters,
        TConfig configuration);

    protected abstract bool TryResolvePaymentId(
        IReadOnlyDictionary<string, string> parameters,
        out Guid paymentId);

    protected abstract bool TryResolveAmount(
        IReadOnlyDictionary<string, string> parameters,
        out long amountVnd);

    protected abstract string ResolveResponseCode(
        IReadOnlyDictionary<string, string> parameters);

    protected abstract string ResolveTransactionStatus(
        IReadOnlyDictionary<string, string> parameters);

    protected abstract bool IsSuccess(string responseCode, string transactionStatus);

    protected virtual string? ResolveGatewayTransactionId(
        IReadOnlyDictionary<string, string> parameters)
    {
        return null;
    }

    protected virtual string GetLedgerEntryDescription(WalletJournal journal) =>
        $"{Gateway} payment {journal.PaymentId}";

    private async Task<WalletJournal?> ResolveJournalAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var journals = await _walletJournalRepository.FindAsync(journal => journal.PaymentId == paymentId, cancellationToken);
        return journals.FirstOrDefault();
    }

    private async Task<Wallet?> ResolveWalletFromLedgerAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var journals = await _walletJournalRepository.FindAsync(journal => journal.PaymentId == paymentId, cancellationToken);
        var journal = journals.FirstOrDefault();
        if (journal is null)
        {
            return null;
        }

        var entries = await _walletLedgerEntryRepository.FindAsync(entry => entry.JournalId == journal.Id, cancellationToken);
        var walletId = entries.FirstOrDefault()?.WalletId;
        if (!walletId.HasValue)
        {
            return null;
        }

        return await FindWalletAsync(walletId.Value, cancellationToken);
    }

    private async Task<Wallet?> FindWalletAsync(Guid walletId, CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.FindAsync(wallet => wallet.Id == walletId, cancellationToken);
        return wallets.FirstOrDefault();
    }

    private async Task<Wallet?> FindWalletByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.FindAsync(wallet => wallet.UserId == userId, cancellationToken);
        return wallets.FirstOrDefault();
    }

    private async Task<WalletResolution> ResolveWalletAsync(
        Guid? userIdHint,
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        Wallet? wallet = null;

        if (userIdHint.HasValue)
        {
            wallet = await FindWalletByUserAsync(userIdHint.Value, cancellationToken);
        }

        if (wallet is null)
        {
            wallet = await ResolveWalletFromLedgerAsync(paymentId, cancellationToken);
            if (wallet is not null && userIdHint.HasValue && wallet.UserId != userIdHint.Value)
            {
                _logger.LogInformation(
                    "{Gateway} return located wallet {WalletId} via ledger for payment {PaymentId} but it belongs to user {WalletUserId}",
                    Gateway,
                    wallet.Id,
                    paymentId,
                    wallet.UserId);
            }
        }

        if (wallet is not null)
        {
            var resolvedUserId = userIdHint ?? wallet.UserId;
            return new WalletResolution(wallet.Id, resolvedUserId, true);
        }

        if (!userIdHint.HasValue)
        {
            _logger.LogWarning("{Gateway} return missing user id for payment {PaymentId}", Gateway, paymentId);
            return WalletResolution.MissingUser;
        }

        return new WalletResolution(null, userIdHint, true);
    }

    private async Task UpdateJournalsAsync(Guid paymentId, WalletJournalStatus status, DateTime? postedAt, CancellationToken cancellationToken)
    {
        var journals = await _walletJournalRepository.FindAsync(journal => journal.PaymentId == paymentId, cancellationToken);
        foreach (var journal in journals)
        {
            journal.Status = status;
            journal.PostedAt = postedAt;
            _walletJournalRepository.Update(journal);
        }
    }

    private readonly record struct WalletResolution(Guid? WalletId, Guid? UserId, bool HasUserId)
    {
        public static WalletResolution MissingUser => new(null, null, false);
    }

    private async Task<Wallet> EnsureWalletAsync(Guid walletId, Guid userId, DateTime timestamp, CancellationToken cancellationToken)
    {
        var wallet = await FindWalletAsync(walletId, cancellationToken);
        if (wallet is not null)
        {
            wallet.UpdatedAt = timestamp;
            _walletRepository.Update(wallet);
            return wallet;
        }

        wallet = new Wallet
        {
            Id = walletId,
            UserId = userId,
            IsSystem = true,
            Status = WalletStatus.Active,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        await _walletRepository.AddAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    private async Task EnsureWalletBalanceAsync(WalletJournal journal, Guid walletId, long amountVnd, DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var accountType = MapJournalTypeToAccountType(journal.Type);
        var balances = await _walletBalanceRepository.FindAsync(
            balance => balance.WalletId == walletId && balance.AccountType == accountType,
            cancellationToken);

        var balance = balances.FirstOrDefault();

        if (balance is null)
        {
            balance = new WalletBalance
            {
                Id = Guid.NewGuid(),
                WalletId = walletId,
                BalanceVnd = amountVnd,
                AccountType = accountType,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            await _walletBalanceRepository.AddAsync(balance, cancellationToken);
            return;
        }

        balance.BalanceVnd += amountVnd;
        balance.UpdatedAt = timestamp;
        _walletBalanceRepository.Update(balance);
    }

    private async Task EnsureLedgerEntryAsync(WalletJournal journal, Guid walletId, long amountVnd, DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var existingEntries =
            await _walletLedgerEntryRepository.FindAsync(entry => entry.JournalId == journal.Id, cancellationToken);
        var entry = existingEntries.FirstOrDefault();
        var dc = MapJournalTypeToDc(journal.Type);
        var accountType = MapJournalTypeToAccountType(journal.Type);
        var description = GetLedgerEntryDescription(journal);

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

    private static Guid? ResolveUserId(
        IReadOnlyDictionary<string, string> parameters,
        Guid? fallback)
    {
        if (parameters.TryGetValue("userId", out var userIdValue) &&
            Guid.TryParse(userIdValue, out var parsedUserId))
        {
            return parsedUserId;
        }

        return fallback;
    }

    private static void MarkPaymentAsFailed(Payment payment)
    {
        payment.Status = PaymentStatus.Failed;
        payment.PaidAt = null;
    }

    private static WalletAccountType MapJournalTypeToAccountType(WalletJournalType journalType)
    {
        return journalType switch
        {
            WalletJournalType.Hold => WalletAccountType.Escrow,
            _ => WalletAccountType.Available
        };
    }

    private static Dc MapJournalTypeToDc(WalletJournalType journalType)
    {
        return journalType switch
        {
            WalletJournalType.Deposit or
                WalletJournalType.Release or
                WalletJournalType.Capture or
                WalletJournalType.Refund or
                WalletJournalType.DisputeRelease => Dc.Debit,

            WalletJournalType.Hold => Dc.Debit,

            WalletJournalType.Payout or
                WalletJournalType.Fee or
                WalletJournalType.DisputeFreeze => Dc.Credit,

            _ => Dc.Debit
        };
    }
}
