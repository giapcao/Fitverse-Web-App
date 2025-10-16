using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachServices.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachServices.Handler;

public sealed class ListAllCoachServicesQueryHandler : IQueryHandler<ListAllCoachServicesQuery, PagedResult<CoachServiceDto>>
{
    private readonly ICoachServiceRepository _repository;

    public ListAllCoachServicesQueryHandler(ICoachServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<CoachServiceDto>>> Handle(
        ListAllCoachServicesQuery request,
        CancellationToken cancellationToken)
    {
        var services = await _repository.GetAllAsync(cancellationToken);
        var dto = services.Select(CoachServiceMapping.ToDto);
        var pagedResult = PagedResult<CoachServiceDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachServiceDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}

