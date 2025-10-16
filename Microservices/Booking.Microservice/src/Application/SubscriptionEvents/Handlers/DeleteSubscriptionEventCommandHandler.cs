using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.SubscriptionEvents.Commands;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.SubscriptionEvents.Handlers;

public sealed class DeleteSubscriptionEventCommandHandler : ICommandHandler<DeleteSubscriptionEventCommand>
{
    private readonly ISubscriptionEventRepository _subscriptionEventRepository;

    public DeleteSubscriptionEventCommandHandler(ISubscriptionEventRepository subscriptionEventRepository)
    {
        _subscriptionEventRepository = subscriptionEventRepository;
    }

    public async Task<Result> Handle(DeleteSubscriptionEventCommand request, CancellationToken cancellationToken)
    {
        var subscriptionEvent = await _subscriptionEventRepository.FindByIdAsync(request.Id, cancellationToken);
        if (subscriptionEvent is null)
        {
            return Result.Failure(SubscriptionEventErrors.NotFound(request.Id));
        }

       _subscriptionEventRepository.Delete(subscriptionEvent);

       return Result.Success();
    }
}
