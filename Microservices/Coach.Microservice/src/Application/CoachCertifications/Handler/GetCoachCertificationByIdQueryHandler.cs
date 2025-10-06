using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachCertifications.Handler;

public sealed class GetCoachCertificationByIdQueryHandler : IQueryHandler<GetCoachCertificationByIdQuery, CoachCertificationDto>
{
    private readonly ICoachCertificationRepository _repository;

    public GetCoachCertificationByIdQueryHandler(ICoachCertificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CoachCertificationDto>> Handle(GetCoachCertificationByIdQuery request, CancellationToken cancellationToken)
    {
        var certification = await _repository.GetDetailedByIdAsync(request.CertificationId, cancellationToken, asNoTracking: true);
        if (certification is null)
        {
            return Result.Failure<CoachCertificationDto>(new Error("CoachCertification.NotFound", $"Coach certification {request.CertificationId} was not found."));
        }

        return Result.Success(CoachCertificationMapping.ToDto(certification));
    }
}
