using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Timeslots.Commands;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Timeslots.Handlers;

public sealed class CreateTimeslotCommandHandler : ICommandHandler<CreateTimeslotCommand, TimeslotDto>
{
    private readonly ITimeslotRepository _timeslotRepository;
    private readonly IMapper _mapper;

    public CreateTimeslotCommandHandler(ITimeslotRepository timeslotRepository, IMapper mapper)
    {
        _timeslotRepository = timeslotRepository;
        _mapper = mapper;
    }

    public async Task<Result<TimeslotDto>> Handle(CreateTimeslotCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var timeslot = new Timeslot
        {
            Id = Guid.NewGuid(),
            CoachId = request.CoachId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Status = request.Status,
            IsOnline = request.IsOnline,
            OnsiteLat = request.OnsiteLat,
            OnsiteLng = request.OnsiteLng,
            Capacity = request.Capacity,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _timeslotRepository.AddAsync(timeslot, cancellationToken);

        var persisted = await _timeslotRepository.GetDetailedByIdAsync(timeslot.Id, cancellationToken, asNoTracking: true) ?? timeslot;
        var dto = _mapper.Map<TimeslotDto>(persisted);
        return Result.Success(dto);
    }
}
