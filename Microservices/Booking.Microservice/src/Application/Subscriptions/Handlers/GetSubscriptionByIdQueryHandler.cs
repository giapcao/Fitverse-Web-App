using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Subscriptions.Queries;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Handlers;

public sealed class GetSubscriptionByIdQueryHandler : IQueryHandler<GetSubscriptionByIdQuery, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public GetSubscriptionByIdQueryHandler(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetDetailedByIdAsync(request.Id, cancellationToken, asNoTracking: true)
                          ?? await _subscriptionRepository.FindByIdAsync(request.Id, cancellationToken, asNoTracking: true);
        if (subscription is null)
        {
            return Result.Failure<SubscriptionDto>(SubscriptionErrors.NotFound(request.Id));
        }

        var dto = _mapper.Map<SubscriptionDto>(subscription);
        return Result.Success(dto);
    }
}
