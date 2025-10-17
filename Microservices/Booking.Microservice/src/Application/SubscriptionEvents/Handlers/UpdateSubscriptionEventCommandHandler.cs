using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.SubscriptionEvents.Commands;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.SubscriptionEvents.Handlers;

public sealed class UpdateSubscriptionEventCommandHandler : ICommandHandler<UpdateSubscriptionEventCommand, SubscriptionEventDto>
{
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IMapper _mapper;

    public UpdateSubscriptionEventCommandHandler(ISubscriptionEventRepository subscriptionEventRepository, IMapper mapper)
    {
        _subscriptionEventRepository = subscriptionEventRepository;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionEventDto>> Handle(UpdateSubscriptionEventCommand request, CancellationToken cancellationToken)
    {
        var subscriptionEvent = await _subscriptionEventRepository.FindByIdAsync(request.Id, cancellationToken);
        if (subscriptionEvent is null)
        {
            return Result.Failure<SubscriptionEventDto>(SubscriptionEventErrors.NotFound(request.Id));
        }

        subscriptionEvent.EventType = request.EventType;
        subscriptionEvent.BookingId = request.BookingId;
        subscriptionEvent.TimeslotId = request.TimeslotId;

        _subscriptionEventRepository.Update(subscriptionEvent);

        var dto = _mapper.Map<SubscriptionEventDto>(subscriptionEvent);
        return Result.Success(dto);
    }
}
