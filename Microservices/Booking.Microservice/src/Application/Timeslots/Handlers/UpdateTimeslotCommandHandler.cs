using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Timeslots.Commands;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Timeslots.Handlers;

public sealed class UpdateTimeslotCommandHandler : ICommandHandler<UpdateTimeslotCommand, TimeslotDto>
{
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly IMapper _mapper;

    public UpdateTimeslotCommandHandler(ITimeslotRepository timeslotRepository, IMapper mapper)
    {
        _timeslotRepository = timeslotRepository;
        _mapper = mapper;
    }

    public async Task<Result<TimeslotDto>> Handle(UpdateTimeslotCommand request, CancellationToken cancellationToken)
    {
        var timeslot = await _timeslotRepository.FindByIdAsync(request.Id, cancellationToken);
        if (timeslot is null)
        {
            return Result.Failure<TimeslotDto>(TimeslotErrors.NotFound(request.Id));
        }

        timeslot.CoachId = request.CoachId;
        timeslot.StartAt = request.StartAt;
        timeslot.EndAt = request.EndAt;
        timeslot.Status = request.Status;
        timeslot.IsOnline = request.IsOnline;
        timeslot.OnsiteLat = request.OnsiteLat;
        timeslot.OnsiteLng = request.OnsiteLng;
        timeslot.Capacity = request.Capacity;
        timeslot.UpdatedAt = DateTime.UtcNow;

        _timeslotRepository.Update(timeslot);

        var persisted = await _timeslotRepository.GetDetailedByIdAsync(timeslot.Id, cancellationToken, asNoTracking: true) ?? timeslot;
        var dto = _mapper.Map<TimeslotDto>(persisted);
        return Result.Success(dto);
    }
}
