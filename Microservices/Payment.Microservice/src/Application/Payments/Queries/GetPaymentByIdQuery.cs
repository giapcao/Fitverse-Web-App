using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using MapsterMapper;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Queries;

public sealed record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentResponse>;

internal sealed class GetPaymentByIdQueryHandler : IQueryHandler<GetPaymentByIdQuery, PaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentResponse>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
            var response = _mapper.Map<PaymentResponse>(payment);
            return Result.Success(response);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<PaymentResponse>(new Error("Payment.NotFound", $"Payment with id {request.Id} was not found."));
        }
    }
}
