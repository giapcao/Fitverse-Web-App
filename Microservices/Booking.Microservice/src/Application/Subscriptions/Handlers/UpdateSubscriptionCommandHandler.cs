using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Subscriptions.Commands;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Handlers;

public sealed class UpdateSubscriptionCommandHandler : ICommandHandler<UpdateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;

    public UpdateSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository, IMapper mapper)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionDto>> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.FindByIdAsync(request.Id, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure<SubscriptionDto>(SubscriptionErrors.NotFound(request.Id));
        }

        subscription.UserId = request.UserId;
        subscription.CoachId = request.CoachId;
        subscription.ServiceId = request.ServiceId;
        subscription.Status = request.Status;
        subscription.PeriodStart = request.PeriodStart;
        subscription.PeriodEnd = request.PeriodEnd;
        subscription.SessionsTotal = request.SessionsTotal;
        subscription.SessionsReserved = request.SessionsReserved;
        subscription.SessionsConsumed = request.SessionsConsumed;
        subscription.PriceGrossVnd = request.PriceGrossVnd;
        subscription.CommissionPct = request.CommissionPct;
        subscription.CommissionVnd = request.CommissionVnd;
        subscription.NetAmountVnd = request.NetAmountVnd;
        subscription.CurrencyCode = request.CurrencyCode;
        subscription.UpdatedAt = DateTime.UtcNow;

        _subscriptionRepository.Update(subscription);

        var persisted = await _subscriptionRepository.GetDetailedByIdAsync(subscription.Id, cancellationToken, asNoTracking: true) ?? subscription;
        var dto = _mapper.Map<SubscriptionDto>(persisted);
        return Result.Success(dto);
    }
}
