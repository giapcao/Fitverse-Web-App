using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachMedia.Handler;

public sealed class DeleteCoachMediaCommandHandler : ICommandHandler<DeleteCoachMediaCommand>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public DeleteCoachMediaCommandHandler(
        ICoachMediaRepository repository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteCoachMediaCommand request, CancellationToken cancellationToken)
    {
        var medium = await _repository.GetDetailedByIdAsync(request.MediaId, cancellationToken);
        if (medium is null)
        {
            return Result.Failure(new Error("CoachMedia.NotFound", $"Coach media {request.MediaId} was not found."));
        }

        if (!string.IsNullOrWhiteSpace(medium.Url))
        {
            await _fileStorageService.DeleteAsync(medium.Url, cancellationToken).ConfigureAwait(false);
        }

        _repository.Delete(medium);

        return Result.Success();
    }
}

