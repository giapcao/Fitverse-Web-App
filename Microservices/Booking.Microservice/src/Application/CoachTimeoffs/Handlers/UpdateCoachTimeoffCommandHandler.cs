using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachTimeoffs.Commands;
using Application.Features;
using Domain.IRepositories;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachTimeoffs.Handlers;

public sealed class UpdateCoachTimeoffCommandHandler : ICommandHandler<UpdateCoachTimeoffCommand, CoachTimeoffDto>
{
    private readonly ICoachTimeoffRepository _coachTimeoffRepository;
    private readonly IMapper _mapper;

    public UpdateCoachTimeoffCommandHandler(ICoachTimeoffRepository coachTimeoffRepository, IMapper mapper)
    {
        _coachTimeoffRepository = coachTimeoffRepository;
        _mapper = mapper;
    }

    public async Task<Result<CoachTimeoffDto>> Handle(UpdateCoachTimeoffCommand request, CancellationToken cancellationToken)
    {
        var timeoff = await _coachTimeoffRepository.FindByIdAsync(request.Id, cancellationToken);
        if (timeoff is null)
        {
            return Result.Failure<CoachTimeoffDto>(CoachTimeoffErrors.NotFound(request.Id));
        }

        timeoff.CoachId = request.CoachId;
        timeoff.StartAt = request.StartAt;
        timeoff.EndAt = request.EndAt;
        timeoff.Reason = request.Reason;

        _coachTimeoffRepository.Update(timeoff);

        var dto = _mapper.Map<CoachTimeoffDto>(timeoff);
        return Result.Success(dto);
    }
}
