using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Common;
using Application.CoachProfiles.Handler;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using Domain.Persistence.Enums;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

public sealed class UpdateRejectedKycStatusCommandHandler : ICommandHandler<UpdateRejectedKycStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _recordRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UpdateRejectedKycStatusCommandHandler> _logger;
    public UpdateRejectedKycStatusCommandHandler(
        IKycRecordRepository recordRepository,
        ICoachProfileRepository profileRepository,
        IFileStorageService fileStorageService,
        IEmailSender emailSender,
        ILogger<UpdateRejectedKycStatusCommandHandler> logger)
    {
        _recordRepository = recordRepository;
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
        _emailSender = emailSender;
        _logger = logger;
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

        var coachEmail = result.Value.Coach.Email;
        if (string.IsNullOrWhiteSpace(coachEmail))
        {
            _logger.LogWarning("Coach {CoachId} has no email on profile; skip rejection notification.", result.Value.Coach.CoachId);
        }
        else
        {
            var coachName = string.IsNullOrWhiteSpace(result.Value.Coach.Fullname)
                ? "Coach"
                : result.Value.Coach.Fullname!;

            var reasons = SplitAdminNote(result.Value.AdminNote);
            var htmlBody = CoachEmailTemplates.BuildCoachRejectedEmail(coachName, reasons);
            const string subject = "Tài khoản Coach - Cần bổ sung thông tin";

            try
            {
                _logger.LogInformation("Sending rejection email to {CoachEmail} for coach {CoachId}.", coachEmail, result.Value.Coach.CoachId);
                await _emailSender.SendAsync(coachEmail, subject, htmlBody, cancellationToken);
                _logger.LogInformation("Rejection email sent to {CoachEmail} for coach {CoachId}.", coachEmail, result.Value.Coach.CoachId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection email to {CoachEmail} for coach {CoachId}.", coachEmail, result.Value.Coach.CoachId);
            }
        }

        var signedCoach = await CoachProfileAvatarHelper.WithSignedAvatarAsync(result.Value.Coach, _fileStorageService, cancellationToken);
        return Result.Success(result.Value with { Coach = signedCoach });
    }

    private static string[] SplitAdminNote(string? adminNote)
    {
        if (string.IsNullOrWhiteSpace(adminNote))
        {
            return Array.Empty<string>();
        }

        return adminNote
            .Split(new[] { '\r', '\n', ';', '•' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToArray();
    }
}
