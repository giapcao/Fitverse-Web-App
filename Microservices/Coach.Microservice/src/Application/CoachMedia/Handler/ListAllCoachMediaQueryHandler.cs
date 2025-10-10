using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachMedia.Handler;

public sealed class ListAllCoachMediaQueryHandler : IQueryHandler<ListAllCoachMediaQuery, PagedResult<CoachMediaDto>>
{
    private readonly ICoachMediaRepository _repository;

    public ListAllCoachMediaQueryHandler(ICoachMediaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<CoachMediaDto>>> Handle(
        ListAllCoachMediaQuery request,
        CancellationToken cancellationToken)
    {
        var media = await _repository.GetAllAsync(cancellationToken);
        var dto = media.Select(CoachMediaMapping.ToDto);
        var pagedResult = PagedResult<CoachMediaDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachMediaDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
