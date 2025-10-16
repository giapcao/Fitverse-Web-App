using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachTimeoffs.Commands;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachTimeoffs.Handlers;

public sealed class DeleteCoachTimeoffCommandHandler : ICommandHandler<DeleteCoachTimeoffCommand>
{
    private readonly ICoachTimeoffRepository _coachTimeoffRepository;

    public DeleteCoachTimeoffCommandHandler(ICoachTimeoffRepository coachTimeoffRepository)
    {
        _coachTimeoffRepository = coachTimeoffRepository;
    }

    public async Task<Result> Handle(DeleteCoachTimeoffCommand request, CancellationToken cancellationToken)
    {
        var timeoff = await _coachTimeoffRepository.FindByIdAsync(request.Id, cancellationToken);
        if (timeoff is null)
        {
            return Result.Failure(CoachTimeoffErrors.NotFound(request.Id));
        }

        _coachTimeoffRepository.Delete(timeoff);

        return Result.Success();
    }
}
