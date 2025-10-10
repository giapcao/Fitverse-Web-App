using System;
using Domain.Enums;

namespace Application.WalletLedgerEntries.Queries;

public sealed record WalletLedgerEntryResponse(
    Guid Id,
    Guid JournalId,
    Guid WalletId,
    long AmountVnd,
    Dc Dc,
    string? Description,
    DateTime CreatedAt);
