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

namespace Application.CoachCertifications.Handler;

public sealed class CreateCoachCertificationCommandHandler : ICommandHandler<CreateCoachCertificationCommand, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _certificationRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCoachCertificationCommandHandler(
        ICoachCertificationRepository certificationRepository,
        ICoachProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _certificationRepository = certificationRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CoachCertificationDto>> Handle(CreateCoachCertificationCommand request, CancellationToken cancellationToken)
    {
        var profileExists = await _profileRepository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (!profileExists)
        {
            return Result.Failure<CoachCertificationDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var utcNow = DateTime.UtcNow;
        var certification = new CoachCertification
        {
            CoachId = request.CoachId,
            CertName = request.CertName,
            Issuer = request.Issuer,
            IssuedOn = request.IssuedOn,
            ExpiresOn = request.ExpiresOn,
            FileUrl = request.FileUrl,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "pending" : request.Status,
            ReviewedBy = null,
            ReviewedAt = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _certificationRepository.AddAsync(certification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _certificationRepository.GetDetailedByIdAsync(certification.Id, cancellationToken, asNoTracking: true) ?? certification;
        return Result.Success(CoachCertificationMapping.ToDto(created));
    }
}



