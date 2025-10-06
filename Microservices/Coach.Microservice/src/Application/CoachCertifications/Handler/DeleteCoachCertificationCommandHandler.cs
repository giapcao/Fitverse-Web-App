using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachCertifications.Handler;

public sealed class DeleteCoachCertificationCommandHandler : ICommandHandler<DeleteCoachCertificationCommand>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCoachCertificationCommandHandler(ICoachCertificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCoachCertificationCommand request, CancellationToken cancellationToken)
    {
        var certification = await _repository.GetDetailedByIdAsync(request.CertificationId, cancellationToken);
        if (certification is null)
        {
            return Result.Failure(new Error("CoachCertification.NotFound", $"Coach certification {request.CertificationId} was not found."));
        }

        _repository.Delete(certification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
