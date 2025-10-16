using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.AvailabilityRules.Commands;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.AvailabilityRules.Handlers;

public sealed class UpdateAvailabilityRuleCommandHandler
    : ICommandHandler<UpdateAvailabilityRuleCommand, AvailabilityRuleDto>
{
    private readonly IAvailabilityRuleRepository _availabilityRuleRepository;
    private readonly IMapper _mapper;

    public UpdateAvailabilityRuleCommandHandler(
        IAvailabilityRuleRepository availabilityRuleRepository,
        IMapper mapper)
    {
        _availabilityRuleRepository = availabilityRuleRepository;
        _mapper = mapper;
    }

    public async Task<Result<AvailabilityRuleDto>> Handle(UpdateAvailabilityRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _availabilityRuleRepository.FindByIdAsync(request.Id, cancellationToken);
        if (rule is null)
        {
            return Result.Failure<AvailabilityRuleDto>(AvailabilityRuleErrors.NotFound(request.Id));
        }

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

        _availabilityRuleRepository.Update(rule);

        var dto = _mapper.Map<AvailabilityRuleDto>(rule);
        return Result.Success(dto);
    }
}
