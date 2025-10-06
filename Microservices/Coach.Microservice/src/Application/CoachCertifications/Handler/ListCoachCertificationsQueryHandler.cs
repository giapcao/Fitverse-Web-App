using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachCertifications.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachCertifications.Handler;

public sealed class ListCoachCertificationsQueryHandler : IQueryHandler<ListCoachCertificationsQuery, IEnumerable<CoachCertificationDto>>
{
    private readonly ICoachCertificationRepository _repository;

    public ListCoachCertificationsQueryHandler(ICoachCertificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<CoachCertificationDto>>> Handle(ListCoachCertificationsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Persistence.Models.CoachCertification> certifications;
        if (request.CoachId.HasValue)
        {
            certifications = await _repository.GetByCoachIdAsync(request.CoachId.Value, cancellationToken);
        }
        else
        {
            certifications = await _repository.GetAllAsync(cancellationToken);
        }

        var dto = certifications.Select(CoachCertificationMapping.ToDto);
        return Result.Success(dto);
    }
}
