using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class ListAllKycRecordsQueryHandler : IQueryHandler<ListAllKycRecordsQuery, PagedResult<KycRecordDto>>
{
    private readonly IKycRecordRepository _repository;

    public ListAllKycRecordsQueryHandler(IKycRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<KycRecordDto>>> Handle(
        ListAllKycRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllAsync(cancellationToken);
        var dto = records.Select(KycRecordMapping.ToDto);
        var pagedResult = PagedResult<KycRecordDto>.Create(dto, request.PageNumber, request.PageSize);
        if (pagedResult.IsFailure)
        {
            return Result.Failure<PagedResult<KycRecordDto>>(pagedResult.Error);
        }

        return Result.Success(pagedResult);
    }
}
