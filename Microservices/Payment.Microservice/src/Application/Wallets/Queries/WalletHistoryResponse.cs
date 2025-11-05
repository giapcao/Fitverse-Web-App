using System;
using System.Collections.Generic;
using Application.WalletLedgerEntries.Queries;
using Domain.Enums;

namespace Application.Wallets.Queries;

public sealed record WalletHistoryResponse(
    Guid WalletId,
    IReadOnlyCollection<WalletHistoryItemResponse> Items);

public sealed record WalletHistoryItemResponse(
    WalletJournalHistoryResponse Journal);

public sealed record WalletJournalHistoryResponse(
    Guid Id,
    Guid? BookingId,
    Guid? PaymentId,
    WalletJournalStatus Status,
    WalletJournalType JournalType,
    DateTime CreatedAt,
    DateTime? PostedAt,
    IReadOnlyCollection<WalletLedgerEntryResponse> LedgerEntries);
