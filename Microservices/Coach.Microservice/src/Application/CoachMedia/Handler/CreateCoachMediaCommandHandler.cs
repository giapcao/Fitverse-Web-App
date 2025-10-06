using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Command;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachMedia.Handler;

public sealed class CreateCoachMediaCommandHandler : ICommandHandler<CreateCoachMediaCommand, CoachMediaDto>
{
    private readonly ICoachMediaRepository _mediaRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCoachMediaCommandHandler(
        ICoachMediaRepository mediaRepository,
        ICoachProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _mediaRepository = mediaRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CoachMediaDto>> Handle(CreateCoachMediaCommand request, CancellationToken cancellationToken)
    {
        var profileExists = await _profileRepository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (!profileExists)
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var medium = new CoachMedium
        {
            CoachId = request.CoachId,
            MediaName = request.MediaName,
            MediaType = request.MediaType,
            Url = request.Url,
            Status = request.Status,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(medium, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _mediaRepository.GetDetailedByIdAsync(medium.Id, cancellationToken, asNoTracking: true) ?? medium;
        return Result.Success(CoachMediaMapping.ToDto(created));
    }
}
