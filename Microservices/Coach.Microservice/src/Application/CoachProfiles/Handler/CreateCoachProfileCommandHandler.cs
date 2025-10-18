using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Command;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Storage;
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachProfiles.Handler;

public sealed class CreateCoachProfileCommandHandler : ICommandHandler<CreateCoachProfileCommand, CoachProfileDto>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public CreateCoachProfileCommandHandler(ICoachProfileRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachProfileDto>> Handle(CreateCoachProfileCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (exists)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.Exists", $"Coach profile {request.CoachId} already exists."));
        }

        var utcNow = DateTime.UtcNow;
        var normalizedEmail = request.Email.Trim();
        var profile = new CoachProfile
        {
            UserId = request.CoachId,
            Fullname = request.Fullname,
            Email = normalizedEmail,
            Bio = request.Bio,
            YearsExperience = request.YearsExperience,
            BasePriceVnd = request.BasePriceVnd,
            ServiceRadiusKm = request.ServiceRadiusKm,
            AvatarUrl = CoachProfileAvatarHelper.DefaultAvatar,
            BirthDate = request.BirthDate,
            WeightKg = request.WeightKg,
            HeightCm = request.HeightCm,
            Gender = request.Gender,
            OperatingLocation = request.OperatingLocation,
            TaxCode = request.TaxCode,
            CitizenId = request.CitizenId,
            CitizenIssueDate = request.CitizenIssueDate,
            CitizenIssuePlace = request.CitizenIssuePlace,
            KycStatus = KycStatus.Pending,
            RatingAvg = 0m,
            RatingCount = 0,
            IsPublic = request.IsPublic,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        await _repository.AddAsync(profile, cancellationToken);

        var created = await _repository.GetDetailedByUserIdAsync(profile.UserId, cancellationToken, asNoTracking: true) ?? profile;
        var dto = CoachProfileMapping.ToDto(created);
        dto = await CoachProfileFileUrlHelper.WithSignedUrlsAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}
