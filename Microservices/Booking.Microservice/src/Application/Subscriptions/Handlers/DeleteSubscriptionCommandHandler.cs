using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Subscriptions.Commands;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Handlers;

public sealed class DeleteSubscriptionCommandHandler : ICommandHandler<DeleteSubscriptionCommand>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public DeleteSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.FindByIdAsync(request.Id, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure(SubscriptionErrors.NotFound(request.Id));
        }

        _subscriptionRepository.Delete(subscription);

        return Result.Success();
    }
}
