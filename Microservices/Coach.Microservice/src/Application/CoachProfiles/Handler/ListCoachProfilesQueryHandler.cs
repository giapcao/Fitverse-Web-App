using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachProfiles.Handler;

public sealed class ListCoachProfilesQueryHandler : IQueryHandler<ListCoachProfilesQuery, IEnumerable<CoachProfileDto>>
{
    private readonly ICoachProfileRepository _repository;

    public ListCoachProfilesQueryHandler(ICoachProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<CoachProfileDto>>> Handle(ListCoachProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllDetailedAsync(cancellationToken);
        var dto = profiles.Select(CoachProfileMapping.ToDto);
        return Result.Success(dto);
    }
}
