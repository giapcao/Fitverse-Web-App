using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachServices.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachServices.Handler;

public sealed class DeleteCoachServiceCommandHandler : ICommandHandler<DeleteCoachServiceCommand>
{
    private readonly ICoachServiceRepository _repository;
    public DeleteCoachServiceCommandHandler(ICoachServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteCoachServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetDetailedByIdAsync(request.ServiceId, cancellationToken);
        if (service is null)
        {
            return Result.Failure(new Error("CoachService.NotFound", $"Coach service {request.ServiceId} was not found."));
        }

        _repository.Delete(service);

        return Result.Success();
    }
}

