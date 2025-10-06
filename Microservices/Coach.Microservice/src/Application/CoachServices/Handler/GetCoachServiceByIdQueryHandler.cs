using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachServices.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachServices.Handler;

public sealed class GetCoachServiceByIdQueryHandler : IQueryHandler<GetCoachServiceByIdQuery, CoachServiceDto>
{
    private readonly ICoachServiceRepository _repository;

    public GetCoachServiceByIdQueryHandler(ICoachServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CoachServiceDto>> Handle(GetCoachServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetDetailedByIdAsync(request.ServiceId, cancellationToken, asNoTracking: true);
        if (service is null)
        {
            return Result.Failure<CoachServiceDto>(new Error("CoachService.NotFound", $"Coach service {request.ServiceId} was not found."));
        }

        return Result.Success(CoachServiceMapping.ToDto(service));
    }
}
