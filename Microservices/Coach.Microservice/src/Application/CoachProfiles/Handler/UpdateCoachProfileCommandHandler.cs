using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Command;
using Application.Features;
using SharedLibrary.Common;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

public sealed class UpdateCoachProfileCommandHandler : ICommandHandler<UpdateCoachProfileCommand, CoachProfileDto>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public UpdateCoachProfileCommandHandler(ICoachProfileRepository repository, IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachProfileDto>> Handle(UpdateCoachProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetDetailedByUserIdAsync(request.CoachId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        profile.Fullname = request.Fullname ?? profile.Fullname;
        profile.Bio = request.Bio ?? profile.Bio;
        profile.YearsExperience = request.YearsExperience ?? profile.YearsExperience;
        profile.BasePriceVnd = request.BasePriceVnd ?? profile.BasePriceVnd;
        profile.ServiceRadiusKm = request.ServiceRadiusKm ?? profile.ServiceRadiusKm;

        profile.BirthDate = request.BirthDate ?? profile.BirthDate;
        profile.WeightKg = request.WeightKg ?? profile.WeightKg;
        profile.HeightCm = request.HeightCm ?? profile.HeightCm;
        profile.Gender = request.Gender ?? profile.Gender;
        profile.OperatingLocation = request.OperatingLocation ?? profile.OperatingLocation;
        profile.TaxCode = request.TaxCode ?? profile.TaxCode;
        profile.CitizenId = request.CitizenId ?? profile.CitizenId;
        profile.CitizenIssueDate = request.CitizenIssueDate ?? profile.CitizenIssueDate;
        profile.CitizenIssuePlace = request.CitizenIssuePlace ?? profile.CitizenIssuePlace;
        if (request.IsPublic.HasValue)
        {
            profile.IsPublic = request.IsPublic.Value;
        }

        if (request.KycStatus.HasValue)
        {
            profile.KycStatus = request.KycStatus.Value;
        }

        profile.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByUserIdAsync(profile.UserId, cancellationToken, asNoTracking: true) ?? profile;
        var dto = CoachProfileMapping.ToDto(updated);
        dto = await CoachProfileAvatarHelper.WithSignedAvatarAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}

