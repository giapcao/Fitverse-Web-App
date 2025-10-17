using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachTimeoffs.Queries;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachTimeoffs.Handlers;

public sealed class ListCoachTimeoffsByCoachQueryHandler : IQueryHandler<ListCoachTimeoffsByCoachQuery, IReadOnlyCollection<CoachTimeoffDto>>
{
    private readonly ICoachTimeoffRepository _coachTimeoffRepository;
    private readonly IMapper _mapper;

    public ListCoachTimeoffsByCoachQueryHandler(ICoachTimeoffRepository coachTimeoffRepository, IMapper mapper)
    {
        _coachTimeoffRepository = coachTimeoffRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<CoachTimeoffDto>>> Handle(ListCoachTimeoffsByCoachQuery request, CancellationToken cancellationToken)
    {
        var timeoffs = await _coachTimeoffRepository.GetByCoachIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        var dto = timeoffs.Select(t => _mapper.Map<CoachTimeoffDto>(t)).ToList();
        return Result.Success<IReadOnlyCollection<CoachTimeoffDto>>(dto);
    }
}
