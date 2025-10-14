using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachMedia.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachMedia.Handler;

public sealed class ListAllCoachMediaQueryHandler : IQueryHandler<ListAllCoachMediaQuery, PagedResult<CoachMediaDto>>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ListAllCoachMediaQueryHandler(ICoachMediaRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<CoachMediaDto>>> Handle(
        ListAllCoachMediaQuery request,
        CancellationToken cancellationToken)
    {
        var media = await _repository.GetAllAsync(cancellationToken);
        var dtoList = media.Select(CoachMediaMapping.ToDto).ToList();
        var signedList = await CoachMediaFileUrlHelper.WithSignedFileUrlsAsync(dtoList, _fileStorageService, cancellationToken).ConfigureAwait(false);
        var pagedResult = PagedResult<CoachMediaDto>.Create(signedList, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachMediaDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
