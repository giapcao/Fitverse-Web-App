using System;
using System.Collections.Generic;
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

        IEnumerable<Domain.Persistence.Models.CoachProfile> filtered = profiles;

        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            filtered = filtered.Where(p => string.Equals(p.Gender, request.Gender, StringComparison.OrdinalIgnoreCase));
        }

        if (request.MinPriceVnd.HasValue)
        {
            filtered = filtered.Where(p => p.BasePriceVnd.HasValue && p.BasePriceVnd.Value >= request.MinPriceVnd.Value);
        }

        if (request.MaxPriceVnd.HasValue)
        {
            filtered = filtered.Where(p => p.BasePriceVnd.HasValue && p.BasePriceVnd.Value <= request.MaxPriceVnd.Value);
        }

        if (request.MinRating.HasValue)
        {
            filtered = filtered.Where(p => p.RatingAvg.HasValue && p.RatingAvg.Value >= request.MinRating.Value);
        }

        List<Domain.Persistence.Models.CoachProfile> prioritized;
        if (!string.IsNullOrWhiteSpace(request.OperatingLocation))
        {
            var normalizedKeyword = request.OperatingLocation.Trim();
            var matches = filtered
                .Where(p => !string.IsNullOrWhiteSpace(p.OperatingLocation) &&
                            p.OperatingLocation.IndexOf(normalizedKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            var others = filtered
                .Where(p => string.IsNullOrWhiteSpace(p.OperatingLocation) ||
                            p.OperatingLocation.IndexOf(normalizedKeyword, StringComparison.OrdinalIgnoreCase) < 0)
                .ToList();

            prioritized = matches
                .Concat(others)
                .OrderByDescending(p => p.RatingAvg ?? 0m)
                .ThenBy(p => p.BasePriceVnd ?? long.MaxValue)
                .ToList();
        }
        else
        {
            prioritized = filtered
                .OrderByDescending(p => p.RatingAvg ?? 0m)
                .ThenBy(p => p.BasePriceVnd ?? long.MaxValue)
                .ToList();
        }

        var dtoList = prioritized.Select(CoachProfileMapping.ToDto).ToList();
        var signedList = await CoachProfileAvatarHelper.WithSignedAvatarsAsync(dtoList, _fileStorageService, cancellationToken).ConfigureAwait(false);
        var pagedResult = PagedResult<CoachProfileDto>.Create(signedList, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<CoachProfileDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
