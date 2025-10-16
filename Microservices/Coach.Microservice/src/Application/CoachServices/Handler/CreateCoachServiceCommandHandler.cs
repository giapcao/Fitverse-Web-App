using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachServices.Command;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachServices.Handler;

public sealed class CreateCoachServiceCommandHandler : ICommandHandler<CreateCoachServiceCommand, CoachServiceDto>
{
    private readonly ICoachServiceRepository _serviceRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly ISportRepository _sportRepository;
    public CreateCoachServiceCommandHandler(
        ICoachServiceRepository serviceRepository,
        ICoachProfileRepository profileRepository,
        ISportRepository sportRepository)
    {
        _serviceRepository = serviceRepository;
        _profileRepository = profileRepository;
        _sportRepository = sportRepository;
    }

    public async Task<Result<CoachServiceDto>> Handle(CreateCoachServiceCommand request, CancellationToken cancellationToken)
    {
        var profileExists = await _profileRepository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (!profileExists)
        {
            return Result.Failure<CoachServiceDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var sport = await _sportRepository.GetByIdAsync(request.SportId, cancellationToken, asNoTracking: true);
        if (sport is null)
        {
            return Result.Failure<CoachServiceDto>(new Error("Sport.NotFound", $"Sport {request.SportId} was not found."));
        }

        var utcNow = DateTime.UtcNow;
        var service = new CoachService
        {
            CoachId = request.CoachId,
            SportId = request.SportId,
            Title = request.Title,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            SessionsTotal = request.SessionsTotal,
            PriceVnd = request.PriceVnd,
            OnlineAvailable = request.OnlineAvailable ?? true,
            OnsiteAvailable = request.OnsiteAvailable ?? true,
            LocationNote = request.LocationNote,
            IsActive = request.IsActive ?? true,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _serviceRepository.AddAsync(service, cancellationToken);

        var created = await _serviceRepository.GetDetailedByIdAsync(service.Id, cancellationToken, asNoTracking: true) ?? service;
        return Result.Success(CoachServiceMapping.ToDto(created));
    }
}

