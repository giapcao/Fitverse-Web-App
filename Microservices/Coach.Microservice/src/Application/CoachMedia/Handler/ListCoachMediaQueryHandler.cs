using System.Collections.Generic;
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

public sealed class ListCoachMediaQueryHandler : IQueryHandler<ListCoachMediaQuery, PagedResult<CoachMediaDto>>
{
    private readonly ICoachMediaRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ListCoachMediaQueryHandler(ICoachMediaRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<CoachMediaDto>>> Handle(ListCoachMediaQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Persistence.Models.CoachMedium> media;
        if (request.CoachId.HasValue && request.IsFeatured.HasValue)
        {
            media = await _repository.GetFeaturedByCoachIdAsync(request.CoachId.Value, request.IsFeatured.Value, cancellationToken);
        }
        else if (request.CoachId.HasValue)
        {
            media = await _repository.GetByCoachIdAsync(request.CoachId.Value, cancellationToken);
        }
        else if (request.IsFeatured.HasValue)
        {
            media = await _repository.GetFeaturedAsync(request.IsFeatured.Value, cancellationToken);
        }
        else
        {
            media = await _repository.GetAllAsync(cancellationToken);
            media = media.OrderByDescending(m => m.IsFeatured).ThenByDescending(m => m.CreatedAt);
        }

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
