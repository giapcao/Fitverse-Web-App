using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachCertifications.Handler;

public sealed class ListAllCoachCertificationsQueryHandler : IQueryHandler<ListAllCoachCertificationsQuery, PagedResult<CoachCertificationDto>>
{
    private readonly ICoachCertificationRepository _repository;

    public ListAllCoachCertificationsQueryHandler(ICoachCertificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<CoachCertificationDto>>> Handle(
        ListAllCoachCertificationsQuery request,
        CancellationToken cancellationToken)
    {
        var certifications = await _repository.GetAllAsync(cancellationToken);
        var dto = certifications.Select(CoachCertificationMapping.ToDto);
        var pagedResult = PagedResult<CoachCertificationDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachCertificationDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
