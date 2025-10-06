using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class CreateKycRecordCommandHandler : ICommandHandler<CreateKycRecordCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _kycRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateKycRecordCommandHandler(
        IKycRecordRepository kycRepository,
        ICoachProfileRepository profileRepository,
        IUnitOfWork unitOfWork)
    {
        _kycRepository = kycRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<KycRecordDto>> Handle(CreateKycRecordCommand request, CancellationToken cancellationToken)
    {
        var profileExists = await _profileRepository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (!profileExists)
        {
            return Result.Failure<KycRecordDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var record = new KycRecord
        {
            CoachId = request.CoachId,
            IdDocumentUrl = request.IdDocumentUrl,
            AdminNote = request.AdminNote,
            Status = KycStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        await _kycRepository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _kycRepository.GetDetailedByIdAsync(record.Id, cancellationToken, asNoTracking: true) ?? record;
        return Result.Success(KycRecordMapping.ToDto(created));
    }
}
