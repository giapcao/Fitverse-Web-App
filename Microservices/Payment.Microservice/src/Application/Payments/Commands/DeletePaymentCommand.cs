using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public sealed record DeletePaymentCommand(Guid Id) : ICommand;

internal sealed class DeletePaymentCommandHandler : ICommandHandler<DeletePaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;

    public DeletePaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        Payment payment;

        try
        {
            payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error("Payment.NotFound", $"Payment with id {request.Id} was not found."));
        }

        _paymentRepository.Delete(payment);

        return Result.Success();
    }
}
