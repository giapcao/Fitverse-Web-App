using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public sealed record InitiatePaymentCommand(
    long AmountVnd,
    Gateway Gateway,
    Guid? BookingId,
    PaymentFlow Flow
) : ICommand<InitiatePaymentResponse>;

public sealed record InitiatePaymentResponse(
    Guid? PaymentId,
    Guid WalletJournalId,
    PaymentStatus PaymentStatus,
    WalletJournalStatus WalletJournalStatus,
    WalletJournalType WalletJournalType);

internal sealed class InitiatePaymentCommandHandler : ICommandHandler<InitiatePaymentCommand, InitiatePaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletJournalRepository _walletJournalRepository;

    public InitiatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IWalletJournalRepository walletJournalRepository)
    {
        _paymentRepository = paymentRepository;
        _walletJournalRepository = walletJournalRepository;
    }

    public async Task<Result<InitiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var journalType = request.Flow switch
        {
            PaymentFlow.DepositWallet => WalletJournalType.Deposit,
            PaymentFlow.BookingByWallet => WalletJournalType.Hold,
            PaymentFlow.Booking => WalletJournalType.Hold,
            _ => (WalletJournalType?)null
        };

        if (!journalType.HasValue)
        {
            return Result.Failure<InitiatePaymentResponse>(
                new Error("Payment.FlowNotSupported", $"Payment flow '{request.Flow}' is not supported."));
        }

        var isBookingByWallet = request.Flow == PaymentFlow.BookingByWallet;
        Payment? payment = null;

        var isDepositWallet = request.Flow == PaymentFlow.DepositWallet;
        if (isDepositWallet && request.BookingId.HasValue)
        {
            return Result.Failure<InitiatePaymentResponse>(new Error("Error", "booking must be null"));
        }
        
        if (!isBookingByWallet)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                AmountVnd = request.AmountVnd,
                Gateway = request.Gateway,
                Status = PaymentStatus.Initiated,
                CreatedAt = now,
                RefundAmountVnd = 0
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
        }

        var walletJournal = new WalletJournal
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            PaymentId = payment?.Id,
            Status = WalletJournalStatus.Pending,
            Type = journalType.Value,
            CreatedAt = now,
            PostedAt = null
        };

        await _walletJournalRepository.AddAsync(walletJournal, cancellationToken);

        var response = new InitiatePaymentResponse(
            payment?.Id,
            walletJournal.Id,
            payment?.Status ?? PaymentStatus.Initiated,
            walletJournal.Status,
            walletJournal.Type);

        return Result.Success(response);
    }
}
