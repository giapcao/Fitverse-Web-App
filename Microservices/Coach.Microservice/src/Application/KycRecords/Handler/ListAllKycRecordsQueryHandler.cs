using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
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
        var dtoList = await KycRecordMapping
            .ToDtoListWithSignedCoachAsync(records, _fileStorageService, cancellationToken)
            .ConfigureAwait(false);
        var pagedResult = PagedResult<KycRecordDto>.Create(dtoList, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<KycRecordDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}

