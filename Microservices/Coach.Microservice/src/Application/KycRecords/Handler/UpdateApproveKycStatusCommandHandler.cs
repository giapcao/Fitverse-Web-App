using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Common;
using Application.CoachProfiles.Handler;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;
using Options = Infrastructure.Common.Options;

namespace Application.KycRecords.Handler;

public sealed class UpdateApproveKycStatusCommandHandler : ICommandHandler<UpdateApproveKycStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _recordRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmailSender _emailSender;
    private readonly Options.CoachAppOptions _coachAppOptions;
    private readonly ILogger<UpdateApproveKycStatusCommandHandler> _logger;

    public UpdateApproveKycStatusCommandHandler(
        IKycRecordRepository recordRepository,
        ICoachProfileRepository profileRepository,
        IFileStorageService fileStorageService,
        IEmailSender emailSender,
        IOptions<Options.CoachAppOptions> coachAppOptions,
        ILogger<UpdateApproveKycStatusCommandHandler> logger)
    {
        _recordRepository = recordRepository;
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
        _emailSender = emailSender;
        _coachAppOptions = coachAppOptions.Value;
        _logger = logger;
    }

    public async Task<Result<KycRecordDto>> Handle(UpdateApproveKycStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await KycRecordStatusUpdater.UpdateAsync(
            _recordRepository,
            _profileRepository,
            request.RecordId,
            KycStatus.Approved,
            request.AdminNote,
            request.ReviewerId,
            cancellationToken);

        if (result.IsFailure || result.Value.Coach is null)
        {
            return result;
        }

        var coachEmail = result.Value.Coach.Email;
        if (string.IsNullOrWhiteSpace(coachEmail))
        {
            _logger.LogWarning("Coach {CoachId} has no email on profile; skip approval notification.", result.Value.Coach.CoachId);
        }
        else
        {
            var coachName = string.IsNullOrWhiteSpace(result.Value.Coach.Fullname)
                ? "Coach"
                : result.Value.Coach.Fullname!;
            var dashboardUrl = string.IsNullOrWhiteSpace(_coachAppOptions.DashboardUrl)
                ? "#"
                : _coachAppOptions.DashboardUrl.Trim();
            var htmlBody = CoachEmailTemplates.BuildCoachApprovedEmail(coachName, dashboardUrl);
            const string subject = "Tài khoản Coach đã được xác thực";

            try
            {
                _logger.LogInformation("Sending approval email to {CoachEmail} for coach {CoachId}.", coachEmail, result.Value.Coach.CoachId);
                await _emailSender.SendAsync(coachEmail, subject, htmlBody, cancellationToken);
                _logger.LogInformation("Approval email sent to {CoachEmail} for coach {CoachId}.", coachEmail, result.Value.Coach.CoachId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval email to {CoachEmail} for coach {CoachId}.", coachEmail, result.Value.Coach.CoachId);
            }
        }

        var signedCoach = await CoachProfileAvatarHelper.WithSignedAvatarAsync(result.Value.Coach, _fileStorageService, cancellationToken);
        return Result.Success(result.Value with { Coach = signedCoach });
    }
}
