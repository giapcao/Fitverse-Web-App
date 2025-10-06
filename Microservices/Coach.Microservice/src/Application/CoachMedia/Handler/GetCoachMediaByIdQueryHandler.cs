using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachMedia.Handler;

public sealed class GetCoachMediaByIdQueryHandler : IQueryHandler<GetCoachMediaByIdQuery, CoachMediaDto>
{
    private readonly ICoachMediaRepository _repository;

    public GetCoachMediaByIdQueryHandler(ICoachMediaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CoachMediaDto>> Handle(GetCoachMediaByIdQuery request, CancellationToken cancellationToken)
    {
        var medium = await _repository.GetDetailedByIdAsync(request.MediaId, cancellationToken, asNoTracking: true);
        if (medium is null)
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachMedia.NotFound", $"Coach media {request.MediaId} was not found."));
        }

        return Result.Success(CoachMediaMapping.ToDto(medium));
    }
}
