using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class UpdateApproveKycStatusCommandHandler : ICommandHandler<UpdateApproveKycStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _recordRepository;
    private readonly ICoachProfileRepository _profileRepository;
    public UpdateApproveKycStatusCommandHandler(
        IKycRecordRepository recordRepository,
        ICoachProfileRepository profileRepository)
    {
        _recordRepository = recordRepository;
        _profileRepository = profileRepository;
    }

    public Task<Result<KycRecordDto>> Handle(UpdateApproveKycStatusCommand request, CancellationToken cancellationToken) =>
        KycRecordStatusUpdater.UpdateAsync(
            _recordRepository,
            _profileRepository,
            request.RecordId,
            KycStatus.Approved,
            request.AdminNote,
            request.ReviewerId,
            cancellationToken);
}

