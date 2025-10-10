using System;
using Domain.Enums;

namespace Application.WalletJournals.Queries;

public sealed record WalletJournalResponse(
    Guid Id,
    Guid? BookingId,
    Guid? PaymentId,
    WalletJournalStatus Status,
    WalletJournalType JournalType,
    DateTime CreatedAt,
    DateTime? PostedAt);
