using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachMedia.Handler;

public sealed class DeleteCoachMediaCommandHandler : ICommandHandler<DeleteCoachMediaCommand>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCoachMediaCommandHandler(ICoachMediaRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCoachMediaCommand request, CancellationToken cancellationToken)
    {
        var medium = await _repository.GetDetailedByIdAsync(request.MediaId, cancellationToken);
        if (medium is null)
        {
            return Result.Failure(new Error("CoachMedia.NotFound", $"Coach media {request.MediaId} was not found."));
        }

        _repository.Delete(medium);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
