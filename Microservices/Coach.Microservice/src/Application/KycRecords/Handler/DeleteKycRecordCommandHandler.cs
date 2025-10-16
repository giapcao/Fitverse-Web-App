using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.KycRecords.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.KycRecords.Handler;

public sealed class DeleteKycRecordCommandHandler : ICommandHandler<DeleteKycRecordCommand>
{
    private readonly IKycRecordRepository _repository;
    public DeleteKycRecordCommandHandler(IKycRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteKycRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetDetailedByIdAsync(request.RecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(new Error("KycRecord.NotFound", $"KYC record {request.RecordId} was not found."));
        }

        _repository.Delete(record);

        return Result.Success();
    }
}

