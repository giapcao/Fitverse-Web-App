using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.AvailabilityRules.Commands;
using Application.AvailabilityRules.Services;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.AvailabilityRules.Handlers;

public sealed class CreateAvailabilityRuleCommandHandler
    : ICommandHandler<CreateAvailabilityRuleCommand, AvailabilityRuleDto>
{
    private readonly IAvailabilityRuleRepository _availabilityRuleRepository;
    private readonly IAvailabilityRuleScheduler _availabilityRuleScheduler;
    private readonly IMapper _mapper;

    public CreateAvailabilityRuleCommandHandler(
        IAvailabilityRuleRepository availabilityRuleRepository,
        IAvailabilityRuleScheduler availabilityRuleScheduler,
        IMapper mapper)
    {
        _availabilityRuleRepository = availabilityRuleRepository;
        _availabilityRuleScheduler = availabilityRuleScheduler;
        _mapper = mapper;
    }

    public async Task<Result<AvailabilityRuleDto>> Handle(CreateAvailabilityRuleCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var rule = new AvailabilityRule
        {
            Id = Guid.NewGuid(),
            CoachId = request.CoachId,
            Weekday = request.Weekday,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            IsOnline = request.IsOnline,
            OnsiteLat = request.OnsiteLat,
            OnsiteLng = request.OnsiteLng,
            Timezone = request.Timezone,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _availabilityRuleRepository.AddAsync(rule, cancellationToken);
        await _availabilityRuleScheduler.EnsureFutureSlotsAsync(rule, cancellationToken);

        var dto = _mapper.Map<AvailabilityRuleDto>(rule);
        return Result.Success(dto);
    }
}
