using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.CoachProfiles.Query;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.CoachProfiles.Handler;

public sealed class ListCoachProfilesQueryHandler : IQueryHandler<ListCoachProfilesQuery, PagedResult<CoachProfileDto>>
{
    private readonly ICoachProfileRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ListCoachProfilesQueryHandler(ICoachProfileRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<CoachProfileDto>>> Handle(ListCoachProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllDetailedAsync(cancellationToken);
        var dtoList = profiles.Select(CoachProfileMapping.ToDto).ToList();
        var signedList = await CoachProfileAvatarHelper.WithSignedAvatarsAsync(dtoList, _fileStorageService, cancellationToken).ConfigureAwait(false);
        var pagedResult = PagedResult<CoachProfileDto>.Create(signedList, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachProfileDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
