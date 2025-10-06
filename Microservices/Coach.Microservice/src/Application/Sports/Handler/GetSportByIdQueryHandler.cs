using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Sports.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Sports.Handler;

public sealed class GetSportByIdQueryHandler : IQueryHandler<GetSportByIdQuery, SportDto>
{
    private readonly ISportRepository _repository;

    public GetSportByIdQueryHandler(ISportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SportDto>> Handle(GetSportByIdQuery request, CancellationToken cancellationToken)
    {
        var sport = await _repository.GetByIdAsync(request.SportId, cancellationToken, asNoTracking: true);
        if (sport is null)
        {
            return Result.Failure<SportDto>(new Error("Sport.NotFound", $"Sport {request.SportId} was not found."));
        }

        return Result.Success(SportMapping.ToDto(sport));
    }
}
