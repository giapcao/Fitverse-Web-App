using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.AvailabilityRules.Queries;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.AvailabilityRules.Handlers;

public sealed class GetAvailabilityRuleByIdQueryHandler
    : IQueryHandler<GetAvailabilityRuleByIdQuery, AvailabilityRuleDto>
{
    private readonly IAvailabilityRuleRepository _availabilityRuleRepository;
    private readonly IMapper _mapper;

    public GetAvailabilityRuleByIdQueryHandler(
        IAvailabilityRuleRepository availabilityRuleRepository,
        IMapper mapper)
    {
        _availabilityRuleRepository = availabilityRuleRepository;
        _mapper = mapper;
    }

    public async Task<Result<AvailabilityRuleDto>> Handle(GetAvailabilityRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _availabilityRuleRepository.FindByIdAsync(request.Id, cancellationToken, asNoTracking: true);
        if (rule is null)
        {
            return Result.Failure<AvailabilityRuleDto>(AvailabilityRuleErrors.NotFound(request.Id));
        }

        var dto = _mapper.Map<AvailabilityRuleDto>(rule);
        return Result.Success(dto);
    }
}
