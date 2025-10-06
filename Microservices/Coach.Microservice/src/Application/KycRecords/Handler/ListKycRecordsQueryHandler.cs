using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class ListKycRecordsQueryHandler : IQueryHandler<ListKycRecordsQuery, IEnumerable<KycRecordDto>>
{
    private readonly IKycRecordRepository _repository;

    public ListKycRecordsQueryHandler(IKycRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<KycRecordDto>>> Handle(ListKycRecordsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Persistence.Models.KycRecord> records;
        if (request.CoachId.HasValue)
        {
            records = await _repository.GetByCoachIdAsync(request.CoachId.Value, cancellationToken);
        }
        else
        {
            records = await _repository.GetAllAsync(cancellationToken);
        }

        var dto = records.Select(KycRecordMapping.ToDto);
        return Result.Success(dto);
    }
}
