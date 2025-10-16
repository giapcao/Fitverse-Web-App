using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Timeslots.Commands;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Timeslots.Handlers;

public sealed class DeleteTimeslotCommandHandler : ICommandHandler<DeleteTimeslotCommand>
{
    private readonly ITimeslotRepository _timeslotRepository;

    public DeleteTimeslotCommandHandler(ITimeslotRepository timeslotRepository)
    {
        _timeslotRepository = timeslotRepository;
    }

    public async Task<Result> Handle(DeleteTimeslotCommand request, CancellationToken cancellationToken)
    {
        var timeslot = await _timeslotRepository.FindByIdAsync(request.Id, cancellationToken);
        if (timeslot is null)
        {
            return Result.Failure(TimeslotErrors.NotFound(request.Id));
        }

        _timeslotRepository.Delete(timeslot);

        return Result.Success();
    }
}
