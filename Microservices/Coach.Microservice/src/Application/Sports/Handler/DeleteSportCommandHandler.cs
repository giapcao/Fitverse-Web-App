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
    public DeleteSportCommandHandler(ISportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteSportCommand request, CancellationToken cancellationToken)
    {
        var sport = await _repository.GetByIdAsync(request.SportId, cancellationToken);
        if (sport is null)
        {
            return Result.Failure(new Error("Sport.NotFound", $"Sport {request.SportId} was not found."));
        }

        _repository.Delete(sport);

        return Result.Success();
    }
}

