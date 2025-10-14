using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachCertifications.Handler;

public sealed class GetCoachCertificationByIdQueryHandler : IQueryHandler<GetCoachCertificationByIdQuery, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public GetCoachCertificationByIdQueryHandler(
        ICoachCertificationRepository repository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachCertificationDto>> Handle(GetCoachCertificationByIdQuery request, CancellationToken cancellationToken)
    {
        var certification = await _repository.GetDetailedByIdAsync(request.CertificationId, cancellationToken, asNoTracking: true);
        if (certification is null)
        {
            return Result.Failure<CoachCertificationDto>(new Error("CoachCertification.NotFound", $"Coach certification {request.CertificationId} was not found."));
        }

        var dto = CoachCertificationMapping.ToDto(certification);
        dto = await CoachCertificationFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);

    }
}
