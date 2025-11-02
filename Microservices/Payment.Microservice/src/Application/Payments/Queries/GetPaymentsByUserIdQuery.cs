using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Queries;

public sealed record GetPaymentsByUserIdQuery(Guid UserId) : IQuery<IEnumerable<PaymentResponse>>;

internal sealed class GetPaymentsByUserIdQueryHandler
    : IQueryHandler<GetPaymentsByUserIdQuery, IEnumerable<PaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentsByUserIdQueryHandler(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PaymentResponse>>> Handle(
        GetPaymentsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var response = _mapper.Map<IEnumerable<PaymentResponse>>(payments);
        return Result.Success(response);
    }
}
