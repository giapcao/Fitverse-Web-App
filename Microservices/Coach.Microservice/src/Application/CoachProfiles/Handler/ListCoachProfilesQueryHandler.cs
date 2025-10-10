using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachProfiles.Handler;

public sealed class ListCoachProfilesQueryHandler : IQueryHandler<ListCoachProfilesQuery, PagedResult<CoachProfileDto>>
{
    private readonly ICoachProfileRepository _repository;

    public ListCoachProfilesQueryHandler(ICoachProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<CoachProfileDto>>> Handle(ListCoachProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllDetailedAsync(cancellationToken);
        var dto = profiles.Select(CoachProfileMapping.ToDto);
        var pagedResult = PagedResult<CoachProfileDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachProfileDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
