using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachProfiles.Handler;

public sealed class GetCoachProfileByIdQueryHandler : IQueryHandler<GetCoachProfileByIdQuery, CoachProfileDto>
{
    private readonly ICoachProfileRepository _repository;

    public GetCoachProfileByIdQueryHandler(ICoachProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CoachProfileDto>> Handle(GetCoachProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetDetailedByUserIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        if (profile is null)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        return Result.Success(CoachProfileMapping.ToDto(profile));
    }
}
