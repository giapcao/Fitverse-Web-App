using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.KycRecords.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class UpdateKycRecordStatusCommandHandler : ICommandHandler<UpdateKycRecordStatusCommand, KycRecordDto>
{
    private readonly IKycRecordRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateKycRecordStatusCommandHandler(IKycRecordRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<KycRecordDto>> Handle(UpdateKycRecordStatusCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetDetailedByIdAsync(request.RecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<KycRecordDto>(new Error("KycRecord.NotFound", $"KYC record {request.RecordId} was not found."));
        }

        record.Status = request.Status;
        record.AdminNote = request.AdminNote ?? record.AdminNote;
        record.ReviewerId = request.ReviewerId;
        record.ReviewedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _repository.GetDetailedByIdAsync(record.Id, cancellationToken, asNoTracking: true) ?? record;
        return Result.Success(KycRecordMapping.ToDto(updated));
    }
}

