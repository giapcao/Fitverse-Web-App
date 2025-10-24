using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
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

