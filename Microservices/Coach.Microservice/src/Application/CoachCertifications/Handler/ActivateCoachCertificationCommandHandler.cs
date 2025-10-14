using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachCertifications.Handler;

public sealed class ActivateCoachCertificationCommandHandler : ICommandHandler<ActivateCoachCertificationCommand, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    private const string ActiveStatus = "active";

    public ActivateCoachCertificationCommandHandler(
        ICoachCertificationRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachCertificationDto>> Handle(ActivateCoachCertificationCommand request, CancellationToken cancellationToken)
    {
        var certification = await _repository.GetDetailedByIdAsync(request.CertificationId, cancellationToken);
        if (certification is null)
        {
            return Result.Failure<CoachCertificationDto>(new Error("CoachCertification.NotFound", $"Coach certification {request.CertificationId} was not found."));
        }

        certification.Status = ActiveStatus;
        certification.UpdatedAt = DateTime.UtcNow;

        if (request.ReviewedBy.HasValue)
        {
            certification.ReviewedBy = request.ReviewedBy;
            certification.ReviewedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByIdAsync(certification.Id, cancellationToken, asNoTracking: true) ?? certification;
        var dto = CoachCertificationMapping.ToDto(updated);
        dto = await CoachCertificationFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);

    }
}
