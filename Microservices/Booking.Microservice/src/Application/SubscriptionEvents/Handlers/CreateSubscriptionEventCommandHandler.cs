using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.SubscriptionEvents.Commands;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.SubscriptionEvents.Handlers;

public sealed class CreateSubscriptionEventCommandHandler : ICommandHandler<CreateSubscriptionEventCommand, SubscriptionEventDto>
{
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;
    private readonly IMapper _mapper;

    public CreateSubscriptionEventCommandHandler(ISubscriptionEventRepository subscriptionEventRepository, IMapper mapper)
    {
        _subscriptionEventRepository = subscriptionEventRepository;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionEventDto>> Handle(CreateSubscriptionEventCommand request, CancellationToken cancellationToken)
    {
        var subscriptionEvent = new SubscriptionEvent
        {
            Id = Guid.NewGuid(),
            SubscriptionId = request.SubscriptionId,
            EventType = request.EventType,
            BookingId = request.BookingId,
            TimeslotId = request.TimeslotId,
            CreatedAt = DateTime.UtcNow
        };

        await _subscriptionEventRepository.AddAsync(subscriptionEvent, cancellationToken);

        var dto = _mapper.Map<SubscriptionEventDto>(subscriptionEvent);
        return Result.Success(dto);
    }
}
