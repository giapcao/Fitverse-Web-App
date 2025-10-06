using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachProfiles.Handler;

public sealed class DeleteCoachProfileCommandHandler : ICommandHandler<DeleteCoachProfileCommand>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCoachProfileCommandHandler(ICoachProfileRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCoachProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetDetailedByUserIdAsync(request.CoachId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        _repository.Delete(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
