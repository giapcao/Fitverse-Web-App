using Domain.Enums;

namespace Domain.Entities;

public partial class WalletJournal
{
    public Guid Id { get; set; }

    public Guid? BookingId { get; set; }

    public Guid? PaymentId { get; set; }

    public WalletJournalStatus Status { get; set; }

    public WalletJournalType Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PostedAt { get; set; }

    public virtual ICollection<WalletLedgerEntry> WalletLedgerEntries { get; set; } = new List<WalletLedgerEntry>();
}
