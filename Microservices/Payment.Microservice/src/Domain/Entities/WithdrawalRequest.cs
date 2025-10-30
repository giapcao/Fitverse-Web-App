using Domain.Enums;

namespace Domain.Entities;

public partial class WithdrawalRequest
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }

    public Guid UserId { get; set; }

    public long AmountVnd { get; set; }

    public WithdrawalRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? HoldWalletJournalId { get; set; }

    public Guid? PayoutWalletJournalId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;

    public virtual WalletJournal? HoldWalletJournal { get; set; }

    public virtual WalletJournal? PayoutWalletJournal { get; set; }
}
