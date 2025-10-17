using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.CoachProfiles.Handler;
using Application.KycRecords.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

public sealed class ListKycRecordsQueryHandler : IQueryHandler<ListKycRecordsQuery, PagedResult<KycRecordDto>>
{
    private readonly IKycRecordRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ListKycRecordsQueryHandler(IKycRecordRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<KycRecordDto>>> Handle(ListKycRecordsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Persistence.Models.KycRecord> records;
        if (request.CoachId.HasValue)
        {
            records = await _repository.GetByCoachIdAsync(request.CoachId.Value, cancellationToken);
        }
        else
        {
            records = await _repository.GetAllDetailedAsync(cancellationToken);
        }

        var dtoList = records.Select(KycRecordMapping.ToDto).ToList();
        for (var i = 0; i < dtoList.Count; i++)
        {
            var coach = dtoList[i].Coach;
            if (coach is not null)
            {
                var signedCoach = await CoachProfileAvatarHelper.WithSignedAvatarAsync(coach, _fileStorageService, cancellationToken).ConfigureAwait(false);
                dtoList[i] = dtoList[i] with { Coach = signedCoach };
            }
        }

        var pagedResult = PagedResult<KycRecordDto>.Create(dtoList, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<KycRecordDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}

