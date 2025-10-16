using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachServices.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachServices.Handler;

public sealed class ListCoachServicesQueryHandler : IQueryHandler<ListCoachServicesQuery, PagedResult<CoachServiceDto>>
{
    private readonly ICoachServiceRepository _repository;

    public ListCoachServicesQueryHandler(ICoachServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<CoachServiceDto>>> Handle(ListCoachServicesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Persistence.Models.CoachService> services;
        if (request.CoachId.HasValue)
        {
            services = await _repository.GetByCoachIdAsync(request.CoachId.Value, cancellationToken);
        }
        else
        {
            services = await _repository.GetAllAsync(cancellationToken);
        }

        if (request.SportId.HasValue)
        {
            services = services.Where(s => s.SportId == request.SportId.Value);
        }

        var dto = services.Select(CoachServiceMapping.ToDto);
        var pagedResult = PagedResult<CoachServiceDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachServiceDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}

