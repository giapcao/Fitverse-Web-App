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
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCoachServiceCommandHandler(ICoachServiceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCoachServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetDetailedByIdAsync(request.ServiceId, cancellationToken);
        if (service is null)
        {
            return Result.Failure(new Error("CoachService.NotFound", $"Coach service {request.ServiceId} was not found."));
        }

        _repository.Delete(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
