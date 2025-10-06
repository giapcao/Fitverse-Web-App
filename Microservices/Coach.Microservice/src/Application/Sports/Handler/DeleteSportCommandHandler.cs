using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Sports.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Sports.Handler;

public sealed class DeleteSportCommandHandler : ICommandHandler<DeleteSportCommand>
{
    private readonly ISportRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSportCommandHandler(ISportRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteSportCommand request, CancellationToken cancellationToken)
    {
        var sport = await _repository.GetByIdAsync(request.SportId, cancellationToken);
        if (sport is null)
        {
            return Result.Failure(new Error("Sport.NotFound", $"Sport {request.SportId} was not found."));
        }

        _repository.Delete(sport);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
