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

public sealed class ListSubscriptionsByUserQueryHandler : IQueryHandler<ListSubscriptionsByUserQuery, IReadOnlyCollection<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public ListSubscriptionsByUserQueryHandler(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<SubscriptionDto>>> Handle(ListSubscriptionsByUserQuery request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByUserIdAsync(request.UserId, cancellationToken, asNoTracking: true);
        var dto = subscriptions.Select(s => _mapper.Map<SubscriptionDto>(s)).ToList();
        return Result.Success<IReadOnlyCollection<SubscriptionDto>>(dto);
    }
}
