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
    private readonly IUnitOfWork _unitOfWork;

    public UpdateApproveKycStatusCommandHandler(
        IKycRecordRepository recordRepository,
        ICoachProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _recordRepository = recordRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<KycRecordDto>> Handle(UpdateApproveKycStatusCommand request, CancellationToken cancellationToken) =>
        KycRecordStatusUpdater.UpdateAsync(
            _recordRepository,
            _profileRepository,
            _unitOfWork,
            request.RecordId,
            KycStatus.Approved,
            request.AdminNote,
            request.ReviewerId,
            cancellationToken);
}
