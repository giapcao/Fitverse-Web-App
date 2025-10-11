using System;
using Domain.Enums;

namespace Application.WalletBalances.Queries;

public sealed record WalletBalanceResponse(
    Guid Id,
    Guid WalletId,
    long BalanceVnd,
    WalletAccountType AccountType,
    DateTime CreatedAt,
    DateTime UpdatedAt);
