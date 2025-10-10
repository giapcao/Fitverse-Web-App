using System;
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

public sealed record CreateWalletBalanceCommand(
    Guid WalletId,
    long BalanceVnd,
    WalletAccountType AccountType = WalletAccountType.Available
) : ICommand<WalletBalanceResponse>;

internal sealed class CreateWalletBalanceCommandHandler : ICommandHandler<CreateWalletBalanceCommand, WalletBalanceResponse>
{
    private readonly IWalletBalanceRepository _walletBalanceRepository;
    private readonly IMapper _mapper;

    public CreateWalletBalanceCommandHandler(IWalletBalanceRepository walletBalanceRepository, IMapper mapper)
    {
        _walletBalanceRepository = walletBalanceRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletBalanceResponse>> Handle(CreateWalletBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = _mapper.Map<WalletBalance>(request);
        var now = DateTime.UtcNow;

        balance.Id = Guid.NewGuid();
        balance.CreatedAt = now;
        balance.UpdatedAt = now;

        await _walletBalanceRepository.AddAsync(balance, cancellationToken);

        var response = _mapper.Map<WalletBalanceResponse>(balance);
        return Result.Success(response);
    }
}
