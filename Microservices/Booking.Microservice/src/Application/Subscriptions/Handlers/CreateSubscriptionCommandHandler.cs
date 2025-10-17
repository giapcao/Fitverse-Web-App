using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Subscriptions.Commands;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Handlers;

public sealed class CreateSubscriptionCommandHandler : ICommandHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public CreateSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CoachId = request.CoachId,
            ServiceId = request.ServiceId,
            Status = request.Status,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            SessionsTotal = request.SessionsTotal,
            SessionsReserved = request.SessionsReserved,
            SessionsConsumed = request.SessionsConsumed,
            PriceGrossVnd = request.PriceGrossVnd,
            CommissionPct = request.CommissionPct,
            CommissionVnd = request.CommissionVnd,
            NetAmountVnd = request.NetAmountVnd,
            CurrencyCode = request.CurrencyCode,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        var persisted = await _subscriptionRepository.GetDetailedByIdAsync(subscription.Id, cancellationToken, asNoTracking: true) ?? subscription;
        var dto = _mapper.Map<SubscriptionDto>(persisted);
        return Result.Success(dto);
    }
}
