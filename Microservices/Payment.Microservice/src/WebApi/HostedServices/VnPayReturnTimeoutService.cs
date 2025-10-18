using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;

namespace WebApi.HostedServices;

public sealed class VnPayReturnTimeoutService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ReturnTimeout = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VnPayReturnTimeoutService> _logger;

    public VnPayReturnTimeoutService(IServiceScopeFactory serviceScopeFactory, ILogger<VnPayReturnTimeoutService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VNPay return timeout service started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingPaymentsAsync(stoppingToken);
                await Task.Delay(PollingInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown.
        }
        finally
        {
            _logger.LogInformation("VNPay return timeout service stopped.");
        }
    }

    private async Task ProcessPendingPaymentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var walletJournalRepository = scope.ServiceProvider.GetRequiredService<IWalletJournalRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cutoff = DateTime.UtcNow.Subtract(ReturnTimeout);
        var pendingPayments = await paymentRepository.FindAsync(
            payment => payment.Status == PaymentStatus.Initiated && payment.CreatedAt <= cutoff,
            cancellationToken);

        var enumerable = pendingPayments as Payment[] ?? pendingPayments.ToArray();
        if (!enumerable.Any())
        {
            return;
        }

        foreach (var payment in enumerable)
        {
            _logger.LogInformation("Marking payment {PaymentId} as failed because return callback was not received within {Timeout} minutes.", payment.Id, ReturnTimeout.TotalMinutes);
            payment.Status = PaymentStatus.Failed;
            payment.PaidAt = null;
            paymentRepository.Update(payment);

            var journals = await walletJournalRepository.FindAsync(journal => journal.PaymentId == payment.Id, cancellationToken);
            foreach (var journal in journals)
            {
                journal.Status = WalletJournalStatus.Cancelled;
                journal.PostedAt = null;
                walletJournalRepository.Update(journal);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
