using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachTimeoffs.Commands;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using MapsterMapper;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachTimeoffs.Handlers;

public sealed class CreateCoachTimeoffCommandHandler : ICommandHandler<CreateCoachTimeoffCommand, CoachTimeoffDto>
{
    private readonly ICoachTimeoffRepository _coachTimeoffRepository;
    private readonly IMapper _mapper;

    public CreateCoachTimeoffCommandHandler(ICoachTimeoffRepository coachTimeoffRepository, IMapper mapper)
    {
        _coachTimeoffRepository = coachTimeoffRepository;
        _mapper = mapper;
    }

    public async Task<Result<CoachTimeoffDto>> Handle(CreateCoachTimeoffCommand request, CancellationToken cancellationToken)
    {
        var timeoff = new CoachTimeoff
        {
            Id = Guid.NewGuid(),
            CoachId = request.CoachId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        };

        await _coachTimeoffRepository.AddAsync(timeoff, cancellationToken);

        var dto = _mapper.Map<CoachTimeoffDto>(timeoff);
        return Result.Success(dto);
    }
}
