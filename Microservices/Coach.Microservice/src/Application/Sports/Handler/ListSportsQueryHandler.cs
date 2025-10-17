using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Sports.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Sports.Handler;

public sealed class ListSportsQueryHandler : IQueryHandler<ListSportsQuery, PagedResult<SportDto>>
{
    private readonly ISportRepository _repository;

    public ListSportsQueryHandler(ISportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<SportDto>>> Handle(ListSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _repository.GetAllAsync(cancellationToken);
        var dto = sports
            .OrderBy(s => s.DisplayName)
            .Select(SportMapping.ToDto);
        var pagedResult = PagedResult<SportDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<SportDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}

