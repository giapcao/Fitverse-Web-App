using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachMedia.Handler;

public sealed class ListCoachMediaQueryHandler : IQueryHandler<ListCoachMediaQuery, IEnumerable<CoachMediaDto>>
{
    private readonly ICoachMediaRepository _repository;

    public ListCoachMediaQueryHandler(ICoachMediaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<CoachMediaDto>>> Handle(ListCoachMediaQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Persistence.Models.CoachMedium> media;
        if (request.CoachId.HasValue)
        {
            media = await _repository.GetByCoachIdAsync(request.CoachId.Value, cancellationToken);
        }
        else
        {
            media = await _repository.GetAllAsync(cancellationToken);
        }

        var dto = media.Select(CoachMediaMapping.ToDto);
        return Result.Success(dto);
    }
}
