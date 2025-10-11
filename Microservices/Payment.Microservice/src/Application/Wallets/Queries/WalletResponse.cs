using System;
using Domain.Enums;

namespace Application.Wallets.Queries;

public sealed record WalletResponse(
    Guid Id,
    Guid UserId,
    bool IsSystem,
    WalletStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
