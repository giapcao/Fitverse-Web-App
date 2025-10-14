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

public sealed class ListAllKycRecordsQueryHandler : IQueryHandler<ListAllKycRecordsQuery, PagedResult<KycRecordDto>>
{
    private readonly IKycRecordRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ListAllKycRecordsQueryHandler(IKycRecordRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<KycRecordDto>>> Handle(
        ListAllKycRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllDetailedAsync(cancellationToken);
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
