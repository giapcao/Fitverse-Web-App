using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Handler;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

public sealed class UpdateKycRecordStatusCommandHandler : ICommandHandler<UpdateKycRecordStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _repository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;
    public UpdateKycRecordStatusCommandHandler(
        IKycRecordRepository repository,
        ICoachProfileRepository profileRepository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<KycRecordDto>> Handle(UpdateKycRecordStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await KycRecordStatusUpdater.UpdateAsync(
            _repository,
            _profileRepository,
            request.RecordId,
            request.Status,
            request.AdminNote,
            request.ReviewerId,
            cancellationToken);

        if (result.IsFailure || result.Value.Coach is null)
        {
            return result;
        }

        var signedCoach = await CoachProfileAvatarHelper.WithSignedAvatarAsync(result.Value.Coach, _fileStorageService, cancellationToken);
        return Result.Success(result.Value with { Coach = signedCoach });
    }
}
