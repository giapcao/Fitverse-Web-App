using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Payments.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Contracts.Payments;

namespace Application.Payments.Commands;

public sealed record CreatePaymentCommand(
    Guid BookingId,
    long AmountVnd,
    string? GatewayTxnId,
    JsonDocument? GatewayMeta,
    DateTime? PaidAt,
    long? RefundAmountVnd,
    Gateway Gateway,
    PaymentStatus Status
) : ICommand<PaymentResponse>;

internal sealed class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, PaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentResponse>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = _mapper.Map<Payment>(request);
        var now = DateTime.UtcNow;

        payment.CreatedAt = now;
        payment.RefundAmountVnd ??= 0;

        await _paymentRepository.AddAsync(payment, cancellationToken);

        var response = _mapper.Map<PaymentResponse>(payment);
        return Result.Success(response);
    }
}
