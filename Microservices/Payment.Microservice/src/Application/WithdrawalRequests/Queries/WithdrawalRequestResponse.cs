using System;
using Domain.Enums;

namespace Application.WithdrawalRequests.Queries;

public sealed record WithdrawalRequestResponse(
    Guid Id,
    Guid WalletId,
    Guid UserId,
    long AmountVnd,
    WithdrawalRequestStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? HoldWalletJournalId,
    Guid? PayoutWalletJournalId,
    DateTime? ApprovedAt,
    DateTime? CompletedAt,
    DateTime? RejectedAt,
    string? RejectionReason);
