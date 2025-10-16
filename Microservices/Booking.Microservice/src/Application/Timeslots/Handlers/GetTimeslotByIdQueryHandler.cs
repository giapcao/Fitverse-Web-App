using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Timeslots.Queries;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.Timeslots.Handlers;

public sealed class GetTimeslotByIdQueryHandler : IQueryHandler<GetTimeslotByIdQuery, TimeslotDto>
{
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly IMapper _mapper;

    public GetTimeslotByIdQueryHandler(ITimeslotRepository timeslotRepository, IMapper mapper)
    {
        _timeslotRepository = timeslotRepository;
        _mapper = mapper;
    }

    public async Task<Result<TimeslotDto>> Handle(GetTimeslotByIdQuery request, CancellationToken cancellationToken)
    {
        var timeslot = await _timeslotRepository.GetDetailedByIdAsync(request.Id, cancellationToken, asNoTracking: true)
                        ?? await _timeslotRepository.FindByIdAsync(request.Id, cancellationToken, asNoTracking: true);
        if (timeslot is null)
        {
            return Result.Failure<TimeslotDto>(TimeslotErrors.NotFound(request.Id));
        }

        var dto = _mapper.Map<TimeslotDto>(timeslot);
        return Result.Success(dto);
    }
}
