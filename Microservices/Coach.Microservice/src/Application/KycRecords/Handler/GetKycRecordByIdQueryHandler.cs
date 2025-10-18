using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

public sealed class GetKycRecordByIdQueryHandler : IQueryHandler<GetKycRecordByIdQuery, KycRecordDto>
{
    private readonly IKycRecordRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public GetKycRecordByIdQueryHandler(IKycRecordRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<KycRecordDto>> Handle(GetKycRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetDetailedByIdAsync(request.RecordId, cancellationToken, asNoTracking: true);
        if (record is null)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.NotFound", $"KYC record {request.RecordId} was not found."));
        }

        var dto = await KycRecordMapping.ToDtoWithSignedCoachAsync(record, _fileStorageService, cancellationToken).ConfigureAwait(false);
        return Result.Success(dto);
    }
}
