using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Queries;

public sealed record GetPaymentsQuery : IQuery<IEnumerable<PaymentResponse>>;

internal sealed class GetPaymentsQueryHandler : IQueryHandler<GetPaymentsQuery, IEnumerable<PaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PaymentResponse>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);
        var response = _mapper.Map<IEnumerable<PaymentResponse>>(payments);
        return Result.Success(response);
    }
}
