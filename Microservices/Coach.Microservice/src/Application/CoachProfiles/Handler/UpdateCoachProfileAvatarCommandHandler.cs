using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

public sealed class UpdateCoachProfileAvatarCommandHandler : ICommandHandler<UpdateCoachProfileAvatarCommand, CoachProfileDto>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IFileStorageService _fileStorageService;
    private const string DefaultAvatar = CoachProfileAvatarHelper.DefaultAvatar;

    public UpdateCoachProfileAvatarCommandHandler(
        ICoachProfileRepository repository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachProfileDto>> Handle(UpdateCoachProfileAvatarCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetDetailedByUserIdAsync(request.CoachId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        if (request.File is null)
        {
            return Result.Failure<CoachProfileDto>(new Error("CoachProfile.InvalidAvatar", "Avatar file must be provided."));
        }

        if (!string.Equals(profile.AvatarUrl, DefaultAvatar, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(profile.AvatarUrl))
        {
            await _fileStorageService.DeleteAsync(profile.AvatarUrl, cancellationToken).ConfigureAwait(false);
        }

        var directory = request.File.Directory ?? request.Directory ?? "avatar";
        var uploadResult = await _fileStorageService.UploadAsync(
            new FileUploadRequest(
                request.CoachId,
                request.File.Content,
                request.File.FileName,
                request.File.ContentType,
                directory),
            cancellationToken).ConfigureAwait(false);

        profile.AvatarUrl = uploadResult.Key;

        profile.UpdatedAt = DateTime.UtcNow;

        var dto = CoachProfileMapping.ToDto(profile);
        dto = await CoachProfileFileUrlHelper.WithSignedUrlsAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}
