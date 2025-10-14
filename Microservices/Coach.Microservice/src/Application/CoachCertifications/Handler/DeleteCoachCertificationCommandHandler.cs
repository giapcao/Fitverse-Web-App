using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachCertifications.Handler;

public sealed class DeleteCoachCertificationCommandHandler : ICommandHandler<DeleteCoachCertificationCommand>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DeleteCoachCertificationCommandHandler(
        ICoachCertificationRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteCoachCertificationCommand request, CancellationToken cancellationToken)
    {
        var certification = await _repository.GetDetailedByIdAsync(request.CertificationId, cancellationToken);
        if (certification is null)
        {
            return Result.Failure(new Error("CoachCertification.NotFound", $"Coach certification {request.CertificationId} was not found."));
        }

        if (!string.IsNullOrWhiteSpace(certification.FileUrl))
        {
            await _fileStorageService.DeleteAsync(certification.FileUrl, cancellationToken).ConfigureAwait(false);
        }

        _repository.Delete(certification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
