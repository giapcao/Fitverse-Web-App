using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Handler;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

public sealed class UpdateRejectedKycStatusCommandHandler : ICommandHandler<UpdateRejectedKycStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _recordRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;
    public UpdateRejectedKycStatusCommandHandler(
        IKycRecordRepository recordRepository,
        ICoachProfileRepository profileRepository,
        IFileStorageService fileStorageService)
    {
        _recordRepository = recordRepository;
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<KycRecordDto>> Handle(UpdateRejectedKycStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await KycRecordStatusUpdater.UpdateAsync(
            _recordRepository,
            _profileRepository,
            request.RecordId,
            KycStatus.Rejected,
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
