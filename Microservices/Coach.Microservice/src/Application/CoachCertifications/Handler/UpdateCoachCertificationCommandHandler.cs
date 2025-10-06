using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachCertifications.Handler;

public sealed class UpdateCoachCertificationCommandHandler : ICommandHandler<UpdateCoachCertificationCommand, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCoachCertificationCommandHandler(ICoachCertificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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
        certification.FileUrl = request.FileUrl ?? certification.FileUrl;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            certification.Status = request.Status!;
        }

        if (request.ReviewedBy.HasValue)
        {
            certification.ReviewedBy = request.ReviewedBy;
            certification.ReviewedAt = request.ReviewedAt ?? DateTime.UtcNow;
        }
        else if (request.ReviewedAt.HasValue)
        {
            certification.ReviewedAt = request.ReviewedAt;
        }

        certification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByIdAsync(certification.Id, cancellationToken, asNoTracking: true) ?? certification;
        return Result.Success(CoachCertificationMapping.ToDto(updated));
    }
}
