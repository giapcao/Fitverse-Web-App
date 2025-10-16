using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Sports.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Sports.Handler;

public sealed class UpdateSportCommandHandler : ICommandHandler<UpdateSportCommand, SportDto>
{
    private readonly ISportRepository _repository;
    public UpdateSportCommandHandler(ISportRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SportDto>> Handle(UpdateSportCommand request, CancellationToken cancellationToken)
    {
        var sport = await _repository.GetByIdAsync(request.SportId, cancellationToken);
        if (sport is null)
        {
            return Result.Failure<SportDto>(new Error("Sport.NotFound", $"Sport {request.SportId} was not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            sport.DisplayName = request.DisplayName!;
        }

        if (request.Description is not null)
        {
            sport.Description = request.Description;
        }


        var updated = await _repository.GetByIdAsync(sport.Id, cancellationToken, asNoTracking: true) ?? sport;
        return Result.Success(SportMapping.ToDto(updated));
    }
}

