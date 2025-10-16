using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachServices.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachServices.Handler;

public sealed class UpdateCoachServiceCommandHandler : ICommandHandler<UpdateCoachServiceCommand, CoachServiceDto>
{
    private readonly ICoachServiceRepository _repository;
    private readonly ISportRepository _sportRepository;
    public UpdateCoachServiceCommandHandler(ICoachServiceRepository repository, ISportRepository sportRepository)
    {
        _repository = repository;
        _sportRepository = sportRepository;
    }

    public async Task<Result<CoachServiceDto>> Handle(UpdateCoachServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetDetailedByIdAsync(request.ServiceId, cancellationToken);
        if (service is null)
        {
            return Result.Failure<CoachServiceDto>(new Error("CoachService.NotFound", $"Coach service {request.ServiceId} was not found."));
        }

        if (request.SportId.HasValue && request.SportId.Value != service.SportId)
        {
            var sport = await _sportRepository.GetByIdAsync(request.SportId.Value, cancellationToken, asNoTracking: true);
            if (sport is null)
            {
                return Result.Failure<CoachServiceDto>(new Error("Sport.NotFound", $"Sport {request.SportId.Value} was not found."));
            }

            service.SportId = request.SportId.Value;
        }

        service.Title = request.Title ?? service.Title;
        service.Description = request.Description ?? service.Description;
        service.DurationMinutes = request.DurationMinutes ?? service.DurationMinutes;
        service.SessionsTotal = request.SessionsTotal ?? service.SessionsTotal;
        service.PriceVnd = request.PriceVnd ?? service.PriceVnd;

        if (request.OnlineAvailable.HasValue)
        {
            service.OnlineAvailable = request.OnlineAvailable.Value;
        }

        if (request.OnsiteAvailable.HasValue)
        {
            service.OnsiteAvailable = request.OnsiteAvailable.Value;
        }

        service.LocationNote = request.LocationNote ?? service.LocationNote;

        if (request.IsActive.HasValue)
        {
            service.IsActive = request.IsActive.Value;
        }

        service.UpdatedAt = DateTime.UtcNow;


        var updated = await _repository.GetDetailedByIdAsync(service.Id, cancellationToken, asNoTracking: true) ?? service;
        return Result.Success(CoachServiceMapping.ToDto(updated));
    }
}

