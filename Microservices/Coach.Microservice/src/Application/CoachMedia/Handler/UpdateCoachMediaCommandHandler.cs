using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachMedia.Handler;

public sealed class UpdateCoachMediaCommandHandler : ICommandHandler<UpdateCoachMediaCommand, CoachMediaDto>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public UpdateCoachMediaCommandHandler(
        ICoachMediaRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<CoachMediaDto>> Handle(UpdateCoachMediaCommand request, CancellationToken cancellationToken)
    {
        var medium = await _repository.GetDetailedByIdAsync(request.MediaId, cancellationToken);
        if (medium is null)
        {
            return Result.Failure<CoachMediaDto>(new Error("CoachMedia.NotFound", $"Coach media {request.MediaId} was not found."));
        }

        medium.MediaName = request.MediaName ?? medium.MediaName;
        if (request.Description is not null)
        {
            medium.Description = request.Description;
        }

        if (request.MediaType.HasValue)
        {
            medium.MediaType = request.MediaType.Value;
        }

        if (request.File is not null)
        {
            if (!string.IsNullOrWhiteSpace(medium.Url))
            {
                await _fileStorageService.DeleteAsync(medium.Url, cancellationToken).ConfigureAwait(false);
            }

            var directory = request.File.Directory ?? request.Directory ?? "media";
            var uploadResult = await _fileStorageService.UploadAsync(
                new FileUploadRequest(
                    medium.CoachId,
                    request.File.Content,
                    request.File.FileName,
                    request.File.ContentType,
                    directory),
                cancellationToken).ConfigureAwait(false);

            medium.Url = uploadResult.Key;
        }
        else if (!string.IsNullOrWhiteSpace(request.Url))
        {
            medium.Url = request.Url!;
        }


        if (request.IsFeatured.HasValue)
        {
            medium.IsFeatured = request.IsFeatured.Value;
        }

        medium.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByIdAsync(medium.Id, cancellationToken, asNoTracking: true) ?? medium;
        var dto = CoachMediaMapping.ToDto(updated);
        dto = await CoachMediaFileUrlHelper.WithSignedFileUrlAsync(dto, _fileStorageService, cancellationToken);
        return Result.Success(dto);
    }
}
