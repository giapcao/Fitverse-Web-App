using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Subscriptions.Queries;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Handlers;

public sealed class ListSubscriptionsByCoachQueryHandler : IQueryHandler<ListSubscriptionsByCoachQuery, IReadOnlyCollection<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public ListSubscriptionsByCoachQueryHandler(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<SubscriptionDto>>> Handle(ListSubscriptionsByCoachQuery request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByCoachIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        var dto = subscriptions.Select(s => _mapper.Map<SubscriptionDto>(s)).ToList();
        return Result.Success<IReadOnlyCollection<SubscriptionDto>>(dto);
    }
}
