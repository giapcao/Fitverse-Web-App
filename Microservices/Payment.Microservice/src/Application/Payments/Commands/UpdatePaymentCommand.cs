using System;
using System.Collections.Generic;
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

namespace Application.Payments.Commands;

public sealed record UpdatePaymentCommand(
    Guid Id,
    Guid BookingId,
    long AmountVnd,
    string? GatewayTxnId,
    JsonDocument? GatewayMeta,
    DateTime? PaidAt,
    long? RefundAmountVnd,
    Gateway Gateway,
    PaymentStatus Status
) : ICommand<PaymentResponse>;

internal sealed class UpdatePaymentCommandHandler : ICommandHandler<UpdatePaymentCommand, PaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public UpdatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentResponse>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        Payment payment;

        try
        {
            payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<PaymentResponse>(new Error("Payment.NotFound", $"Payment with id {request.Id} was not found."));
        }

        _mapper.Map(request, payment);
        _paymentRepository.Update(payment);

        var response = _mapper.Map<PaymentResponse>(payment);
        return Result.Success(response);
    }
}
