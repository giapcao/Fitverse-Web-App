using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Command;
using Application.Features;
using Domain.IRepositories;
using Domain.Persistence.Models;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachMedia.Handler;

public sealed class CreateCoachMediaCommandHandler : ICommandHandler<CreateCoachMediaCommand, CoachMediaDto>
{
    private readonly ICoachMediaRepository _mediaRepository;
    private readonly ICoachProfileRepository _profileRepository;
    private readonly IFileStorageService _fileStorageService;

    public CreateCoachMediaCommandHandler(
        ICoachMediaRepository mediaRepository,
        ICoachProfileRepository profileRepository,
        IFileStorageService fileStorageService)
    {
        _mediaRepository = mediaRepository;
        _profileRepository = profileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachMediaDto>> Handle(CreateCoachMediaCommand request, CancellationToken cancellationToken)
    {
        var profileExists = await _profileRepository.ExistsByUserIdAsync(request.CoachId, cancellationToken);
        if (!profileExists)
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachProfile.NotFound", $"Coach profile {request.CoachId} was not found."));
        }

        var mediaKey = request.Url;
        if (request.File is not null)
        {
            var directory = request.File.Directory ?? request.Directory ?? "media";
            var uploadResult = await _fileStorageService.UploadAsync(
                new FileUploadRequest(
                    request.CoachId,
                    request.File.Content,
                    request.File.FileName,
                    request.File.ContentType,
                    directory),
                cancellationToken).ConfigureAwait(false);
            mediaKey = uploadResult.Key;
        }

        if (string.IsNullOrWhiteSpace(mediaKey))
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachMedia.InvalidInput", "Either file or URL must be provided."));
        }

        var medium = new CoachMedium
        {
            CoachId = request.CoachId,
            MediaName = request.MediaName,
            Description = request.Description,
            MediaType = request.MediaType,
            Url = mediaKey,
            Status = true,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(medium, cancellationToken);

        var created = await _mediaRepository.GetDetailedByIdAsync(medium.Id, cancellationToken, asNoTracking: true) ?? medium;
        var dto = CoachMediaMapping.ToDto(created);
        dto = await CoachMediaFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}

