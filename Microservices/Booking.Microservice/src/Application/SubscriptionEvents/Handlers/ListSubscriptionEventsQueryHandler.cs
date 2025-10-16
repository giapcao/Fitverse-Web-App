using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.SubscriptionEvents.Queries;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.SubscriptionEvents.Handlers;

public sealed class ListSubscriptionEventsQueryHandler : IQueryHandler<ListSubscriptionEventsQuery, IReadOnlyCollection<SubscriptionEventDto>>
{
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IMapper _mapper;

    public ListSubscriptionEventsQueryHandler(ISubscriptionEventRepository subscriptionEventRepository, IMapper mapper)
    {
        _subscriptionEventRepository = subscriptionEventRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<SubscriptionEventDto>>> Handle(ListSubscriptionEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _subscriptionEventRepository.GetAllAsync(cancellationToken);
        var dto = events.Select(e => _mapper.Map<SubscriptionEventDto>(e)).ToList();
        return Result.Success<IReadOnlyCollection<SubscriptionEventDto>>(dto);
    }
}
