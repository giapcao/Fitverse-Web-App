using Domain.Enums;

namespace Domain.Entities;

public partial class WalletLedgerEntry
{
    public Guid Id { get; set; }

    public Guid JournalId { get; set; }

    public Guid WalletId { get; set; }

    public long AmountVnd { get; set; }

    public Dc Dc { get; set; }
    
    public WalletAccountType AccountType { get; set; }
    
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual WalletJournal Journal { get; set; } = null!;

    public virtual Wallet Wallet { get; set; } = null!;
}
