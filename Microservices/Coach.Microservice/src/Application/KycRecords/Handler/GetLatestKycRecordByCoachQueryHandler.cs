using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Query;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.KycRecords.Handler;

public sealed class GetLatestKycRecordByCoachQueryHandler : IQueryHandler<GetLatestKycRecordByCoachQuery, KycRecordDto>
{
    private readonly IKycRecordRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public GetLatestKycRecordByCoachQueryHandler(IKycRecordRepository repository, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<KycRecordDto>> Handle(GetLatestKycRecordByCoachQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetLatestByCoachIdAsync(request.CoachId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.NotFound", $"No KYC record found for coach {request.CoachId}."));
        }

        var dto = await KycRecordMapping.ToDtoWithSignedCoachAsync(record, _fileStorageService, cancellationToken).ConfigureAwait(false);
        return Result.Success(dto);
    }
}
