using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class GetKycRecordByIdQueryHandler : IQueryHandler<GetKycRecordByIdQuery, KycRecordDto>
{
    private readonly IKycRecordRepository _repository;

    public GetKycRecordByIdQueryHandler(IKycRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<KycRecordDto>> Handle(GetKycRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetDetailedByIdAsync(request.RecordId, cancellationToken, asNoTracking: true);
        if (record is null)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.NotFound", $"KYC record {request.RecordId} was not found."));
        }

        return Result.Success(KycRecordMapping.ToDto(record));
    }
}
