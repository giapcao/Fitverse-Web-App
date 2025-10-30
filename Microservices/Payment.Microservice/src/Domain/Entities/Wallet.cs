using Domain.Enums;

namespace Domain.Entities;

public partial class Wallet
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public bool IsSystem { get; set; }

    public WalletStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<WalletBalance> WalletBalances { get; set; } = new List<WalletBalance>();

    public virtual ICollection<WalletLedgerEntry> WalletLedgerEntries { get; set; } = new List<WalletLedgerEntry>();

    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}
