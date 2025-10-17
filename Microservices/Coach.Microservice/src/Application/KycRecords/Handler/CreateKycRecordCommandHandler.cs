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
    private const string DefaultAdminNote = "kyc_coach_profile";

    public CreateKycRecordCommandHandler(
        IKycRecordRepository kycRepository,
        ICoachProfileRepository profileRepository)
    {
        _kycRepository = kycRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result<KycRecordDto>> Handle(CreateKycRecordCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetDetailedByUserIdAsync(request.CoachId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<KycRecordDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var adminNote = string.IsNullOrWhiteSpace(request.AdminNote) ? DefaultAdminNote : request.AdminNote;

        var record = new KycRecord
        {
            CoachId = request.CoachId,
            IdDocumentUrl = request.IdDocumentUrl,
            AdminNote = adminNote,
            Status = KycStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        profile.KycStatus = record.Status;
        profile.KycNote = adminNote;
        profile.UpdatedAt = DateTime.UtcNow;

        await _kycRepository.AddAsync(record, cancellationToken);

        var created = await _kycRepository.GetDetailedByIdAsync(record.Id, cancellationToken, asNoTracking: true) ?? record;
        return Result.Success(KycRecordMapping.ToDto(created));
    }
}

