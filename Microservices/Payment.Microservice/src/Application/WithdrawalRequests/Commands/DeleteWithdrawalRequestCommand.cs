using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Repositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.WithdrawalRequests.Commands;

public sealed record DeleteWithdrawalRequestCommand(Guid Id) : ICommand;

internal sealed class DeleteWithdrawalRequestCommandHandler : ICommandHandler<DeleteWithdrawalRequestCommand>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

    public DeleteWithdrawalRequestCommandHandler(IWithdrawalRequestRepository withdrawalRequestRepository)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
    }

    public async Task<Result> Handle(DeleteWithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        WithdrawalRequest withdrawalRequest;

        try
        {
            withdrawalRequest = await _withdrawalRequestRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(new Error(
                "WithdrawalRequest.NotFound",
                $"Withdrawal request with id {request.Id} was not found."));
        }

        _withdrawalRequestRepository.Delete(withdrawalRequest);

        return Result.Success();
    }
}
