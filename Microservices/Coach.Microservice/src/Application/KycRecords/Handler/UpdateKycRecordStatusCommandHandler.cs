using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class UpdateKycRecordStatusCommandHandler : ICommandHandler<UpdateKycRecordStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _repository;
    private readonly ICoachProfileRepository _profileRepository;
    public UpdateKycRecordStatusCommandHandler(
        IKycRecordRepository repository,
        ICoachProfileRepository profileRepository)
    {
        _repository = repository;
        _profileRepository = profileRepository;
    }

    public Task<Result<KycRecordDto>> Handle(UpdateKycRecordStatusCommand request, CancellationToken cancellationToken) =>
        KycRecordStatusUpdater.UpdateAsync(
            _repository,
            _profileRepository,
            request.RecordId,
            request.Status,
            request.AdminNote,
            request.ReviewerId,
            cancellationToken);
}

