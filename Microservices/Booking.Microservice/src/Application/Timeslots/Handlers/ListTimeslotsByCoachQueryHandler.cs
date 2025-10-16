using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Timeslots.Queries;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Timeslots.Handlers;

public sealed class ListTimeslotsByCoachQueryHandler : IQueryHandler<ListTimeslotsByCoachQuery, IReadOnlyCollection<TimeslotDto>>
{
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly IMapper _mapper;

    public ListTimeslotsByCoachQueryHandler(ITimeslotRepository timeslotRepository, IMapper mapper)
    {
        _timeslotRepository = timeslotRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<TimeslotDto>>> Handle(ListTimeslotsByCoachQuery request, CancellationToken cancellationToken)
    {
        var timeslots = await _timeslotRepository.GetByCoachIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        var dto = timeslots.Select(t => _mapper.Map<TimeslotDto>(t)).ToList();
        return Result.Success<IReadOnlyCollection<TimeslotDto>>(dto);
    }
}
