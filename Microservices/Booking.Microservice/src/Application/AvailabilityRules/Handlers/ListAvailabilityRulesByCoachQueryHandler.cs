using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.AvailabilityRules.Queries;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.AvailabilityRules.Handlers;

public sealed class ListAvailabilityRulesByCoachQueryHandler
    : IQueryHandler<ListAvailabilityRulesByCoachQuery, IReadOnlyCollection<AvailabilityRuleDto>>
{
    private readonly IAvailabilityRuleRepository _availabilityRuleRepository;
    private readonly IMapper _mapper;

    public ListAvailabilityRulesByCoachQueryHandler(
        IAvailabilityRuleRepository availabilityRuleRepository,
        IMapper mapper)
    {
        _availabilityRuleRepository = availabilityRuleRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<AvailabilityRuleDto>>> Handle(ListAvailabilityRulesByCoachQuery request, CancellationToken cancellationToken)
    {
        var rules = await _availabilityRuleRepository.GetByCoachIdAsync(request.CoachId, cancellationToken, asNoTracking: true);
        var dto = rules.Select(rule => _mapper.Map<AvailabilityRuleDto>(rule)).ToList();
        return Result.Success<IReadOnlyCollection<AvailabilityRuleDto>>(dto);
    }
}
