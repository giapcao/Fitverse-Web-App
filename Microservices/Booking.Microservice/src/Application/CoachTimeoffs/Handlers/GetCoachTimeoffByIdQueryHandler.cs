using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachTimeoffs.Queries;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachTimeoffs.Handlers;

public sealed class GetCoachTimeoffByIdQueryHandler : IQueryHandler<GetCoachTimeoffByIdQuery, CoachTimeoffDto>
{
    private readonly ICoachTimeoffRepository _coachTimeoffRepository;
    private readonly IMapper _mapper;

    public GetCoachTimeoffByIdQueryHandler(ICoachTimeoffRepository coachTimeoffRepository, IMapper mapper)
    {
        _coachTimeoffRepository = coachTimeoffRepository;
        _mapper = mapper;
    }

    public async Task<Result<CoachTimeoffDto>> Handle(GetCoachTimeoffByIdQuery request, CancellationToken cancellationToken)
    {
        var timeoff = await _coachTimeoffRepository.FindByIdAsync(request.Id, cancellationToken, asNoTracking: true);
        if (timeoff is null)
        {
            return Result.Failure<CoachTimeoffDto>(CoachTimeoffErrors.NotFound(request.Id));
        }

        var dto = _mapper.Map<CoachTimeoffDto>(timeoff);
        return Result.Success(dto);
    }
}
