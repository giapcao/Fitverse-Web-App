using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WithdrawalRequests.Queries;

public sealed record GetWithdrawalRequestByIdQuery(Guid Id) : IQuery<WithdrawalRequestResponse>;

internal sealed class GetWithdrawalRequestByIdQueryHandler
    : IQueryHandler<GetWithdrawalRequestByIdQuery, WithdrawalRequestResponse>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestByIdQueryHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IMapper mapper)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _mapper = mapper;
    }

    public async Task<Result<WithdrawalRequestResponse>> Handle(
        GetWithdrawalRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var withdrawalRequest = await _withdrawalRequestRepository.GetByIdAsync(request.Id, cancellationToken);
            var response = _mapper.Map<WithdrawalRequestResponse>(withdrawalRequest);
            return Result.Success(response);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WithdrawalRequestResponse>(new Error(
                "WithdrawalRequest.NotFound",
                $"Withdrawal request with id {request.Id} was not found."));
        }
    }
}
