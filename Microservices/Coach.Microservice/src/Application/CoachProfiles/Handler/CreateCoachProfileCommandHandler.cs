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
using SharedLibrary.Common.ResponseModel;

namespace Application.CoachProfiles.Handler;

public sealed class CreateCoachProfileCommandHandler : ICommandHandler<CreateCoachProfileCommand, CoachProfileDto>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCoachProfileCommandHandler(ICoachProfileRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CoachProfileDto>> Handle(CreateCoachProfileCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (exists)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.Exists", $"Coach profile {request.CoachId} already exists."));
        }

        var utcNow = DateTime.UtcNow;
        var profile = new CoachProfile
        {
            UserId = request.CoachId,
            Fullname = request.Fullname,
            Bio = request.Bio,
            YearsExperience = request.YearsExperience,
            BasePriceVnd = request.BasePriceVnd,
            ServiceRadiusKm = request.ServiceRadiusKm,
            AvatarUrl = request.AvatarUrl,
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetDetailedByUserIdAsync(profile.UserId, cancellationToken, asNoTracking: true) ?? profile;
        return Result.Success(CoachProfileMapping.ToDto(created));
    }
}
