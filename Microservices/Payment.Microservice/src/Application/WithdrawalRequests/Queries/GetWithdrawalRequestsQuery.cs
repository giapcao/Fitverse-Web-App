using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WithdrawalRequests.Queries;

public sealed record GetWithdrawalRequestsQuery : IQuery<IEnumerable<WithdrawalRequestResponse>>;

internal sealed class GetWithdrawalRequestsQueryHandler
    : IQueryHandler<GetWithdrawalRequestsQuery, IEnumerable<WithdrawalRequestResponse>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestsQueryHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IMapper mapper)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WithdrawalRequestResponse>>> Handle(
        GetWithdrawalRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var withdrawalRequests = await _withdrawalRequestRepository.GetAllAsync(cancellationToken);
        var response = _mapper.Map<IEnumerable<WithdrawalRequestResponse>>(withdrawalRequests);
        return Result.Success(response);
    }
}
