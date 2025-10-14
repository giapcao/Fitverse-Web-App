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

public sealed class UpdateCoachCertificationCommandHandler : ICommandHandler<UpdateCoachCertificationCommand, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public UpdateCoachCertificationCommandHandler(
        ICoachCertificationRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachCertificationDto>> Handle(UpdateCoachCertificationCommand request, CancellationToken cancellationToken)
    {
        var certification = await _repository.GetDetailedByIdAsync(request.CertificationId, cancellationToken);
        if (certification is null)
        {
            return Result.Failure<CoachCertificationDto>(new Error("CoachCertification.NotFound", $"Coach certification {request.CertificationId} was not found."));
        }

        certification.CertName = request.CertName ?? certification.CertName;
        certification.Issuer = request.Issuer ?? certification.Issuer;
        certification.IssuedOn = request.IssuedOn ?? certification.IssuedOn;
        certification.ExpiresOn = request.ExpiresOn ?? certification.ExpiresOn;

        if (request.File is not null)
        {
            if (!string.IsNullOrWhiteSpace(certification.FileUrl))
            {
                await _fileStorageService.DeleteAsync(certification.FileUrl, cancellationToken).ConfigureAwait(false);
            }

            var directory = request.File.Directory ?? request.Directory ?? "certifications";
            var uploadResult = await _fileStorageService.UploadAsync(
                new FileUploadRequest(
                    certification.CoachId,
                    request.File.Content,
                    request.File.FileName,
                    request.File.ContentType,
                    directory),
                cancellationToken).ConfigureAwait(false);

            certification.FileUrl = uploadResult.Key;
        }
        else if (!string.IsNullOrWhiteSpace(request.FileUrl))
        {
            certification.FileUrl = request.FileUrl;
        }

        certification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByIdAsync(certification.Id, cancellationToken, asNoTracking: true) ?? certification;
        var dto = CoachCertificationMapping.ToDto(updated);
        dto = await CoachCertificationFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}
