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

public sealed class UpdateAvailabilityRuleCommandHandler
    : ICommandHandler<UpdateAvailabilityRuleCommand, AvailabilityRuleDto>
{
    private readonly IAvailabilityRuleRepository _availabilityRuleRepository;
    private readonly IAvailabilityRuleScheduler _availabilityRuleScheduler;
    private readonly IMapper _mapper;

    public UpdateAvailabilityRuleCommandHandler(
        IAvailabilityRuleRepository availabilityRuleRepository,
        IAvailabilityRuleScheduler availabilityRuleScheduler,
        IMapper mapper)
    {
        _availabilityRuleRepository = availabilityRuleRepository;
        _availabilityRuleScheduler = availabilityRuleScheduler;
        _mapper = mapper;
    }

    public async Task<Result<AvailabilityRuleDto>> Handle(UpdateAvailabilityRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _availabilityRuleRepository.FindByIdAsync(request.Id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure<AvailabilityRuleDto>(AvailabilityRuleErrors.NotFound(request.Id));
        }

        var previousConfiguration = Snapshot(rule);

        rule.CoachId = request.CoachId;
        rule.Weekday = request.Weekday;
        rule.StartTime = request.StartTime;
        rule.EndTime = request.EndTime;
        rule.SlotDurationMinutes = request.SlotDurationMinutes;
        rule.IsOnline = request.IsOnline;
        rule.OnsiteLat = request.OnsiteLat;
        rule.OnsiteLng = request.OnsiteLng;
        rule.Timezone = request.Timezone;
        rule.UpdatedAt = DateTime.UtcNow;

        await _availabilityRuleScheduler.RemoveFutureOpenSlotsAsync(previousConfiguration, cancellationToken);
        _availabilityRuleRepository.Update(rule);
        await _availabilityRuleScheduler.EnsureFutureSlotsAsync(rule, cancellationToken);

        var dto = _mapper.Map<AvailabilityRuleDto>(rule);
        return Result.Success(dto);
    }

    private static AvailabilityRule Snapshot(AvailabilityRule source)
    {
        return new AvailabilityRule
        {
            Id = source.Id,
            CoachId = source.CoachId,
            Weekday = source.Weekday,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            SlotDurationMinutes = source.SlotDurationMinutes,
            IsOnline = source.IsOnline,
            OnsiteLat = source.OnsiteLat,
            OnsiteLng = source.OnsiteLng,
            Timezone = source.Timezone
        };
    }
}
