using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.SubscriptionEvents.Queries;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.SubscriptionEvents.Handlers;

public sealed class GetSubscriptionEventByIdQueryHandler : IQueryHandler<GetSubscriptionEventByIdQuery, SubscriptionEventDto>
{
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IMapper _mapper;

    public GetSubscriptionEventByIdQueryHandler(ISubscriptionEventRepository subscriptionEventRepository, IMapper mapper)
    {
        _subscriptionEventRepository = subscriptionEventRepository;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionEventDto>> Handle(GetSubscriptionEventByIdQuery request, CancellationToken cancellationToken)
    {
        var subscriptionEvent = await _subscriptionEventRepository.FindByIdAsync(request.Id, cancellationToken, asNoTracking: true);
        if (subscriptionEvent is null)
        {
            return Result.Failure<SubscriptionEventDto>(SubscriptionEventErrors.NotFound(request.Id));
        }

        var dto = _mapper.Map<SubscriptionEventDto>(subscriptionEvent);
        return Result.Success(dto);
    }
}
