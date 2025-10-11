using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.WalletBalances.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using MapsterMapper;
using SharedLibrary.Common.ResponseModel;

namespace Application.WalletBalances.Commands;

public sealed record UpdateWalletBalanceCommand(
    Guid Id,
    Guid WalletId,
    long BalanceVnd,
    WalletAccountType AccountType
) : ICommand<WalletBalanceResponse>;

internal sealed class UpdateWalletBalanceCommandHandler : ICommandHandler<UpdateWalletBalanceCommand, WalletBalanceResponse>
{
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IMapper _mapper;

    public UpdateWalletBalanceCommandHandler(IWalletBalanceRepository walletBalanceRepository, IMapper mapper)
    {
        _walletBalanceRepository = walletBalanceRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletBalanceResponse>> Handle(UpdateWalletBalanceCommand request, CancellationToken cancellationToken)
    {
        WalletBalance balance;

        try
        {
            balance = await _walletBalanceRepository.GetByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<WalletBalanceResponse>(new Error("WalletBalance.NotFound", $"Wallet balance with id {request.Id} was not found."));
        }

        _mapper.Map(request, balance);
        balance.UpdatedAt = DateTime.UtcNow;

        _walletBalanceRepository.Update(balance);

        var response = _mapper.Map<WalletBalanceResponse>(balance);
        return Result.Success(response);
    }
}
