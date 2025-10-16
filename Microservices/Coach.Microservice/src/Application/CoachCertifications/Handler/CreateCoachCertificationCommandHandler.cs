using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Command;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachCertifications.Handler;

public sealed class CreateCoachCertificationCommandHandler : ICommandHandler<CreateCoachCertificationCommand, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _certificationRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;

    public CreateCoachCertificationCommandHandler(
        ICoachCertificationRepository certificationRepository,
        ICoachProfileRepository profileRepository,
        IFileStorageService fileStorageService)
    {
        _certificationRepository = certificationRepository;
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachCertificationDto>> Handle(CreateCoachCertificationCommand request, CancellationToken cancellationToken)
    {
        var profileExists = await _profileRepository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (!profileExists)
        {
            return Result.Failure<CoachCertificationDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var fileKey = request.FileUrl;
        if (request.File is not null)
        {
            var directory = request.File.Directory ?? request.Directory ?? "certifications";
            var uploadResult = await _fileStorageService.UploadAsync(
                new FileUploadRequest(
                    request.CoachId,
                    request.File.Content,
                    request.File.FileName,
                    request.File.ContentType,
                    directory),
                cancellationToken);
            fileKey = uploadResult.Key;
        }

        var utcNow = DateTime.UtcNow;
        var certification = new CoachCertification
        {
            CoachId = request.CoachId,
            CertName = request.CertName,
            Issuer = request.Issuer,
            IssuedOn = request.IssuedOn,
            ExpiresOn = request.ExpiresOn,
            FileUrl = fileKey,
            ReviewedBy = null,
            ReviewedAt = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _certificationRepository.AddAsync(certification, cancellationToken);

        var created = await _certificationRepository.GetDetailedByIdAsync(certification.Id, cancellationToken, asNoTracking: true) ?? certification;
        var dto = CoachCertificationMapping.ToDto(created);
        dto = await CoachCertificationFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}



