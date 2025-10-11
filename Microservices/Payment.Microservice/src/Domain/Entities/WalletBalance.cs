using Domain.Enums;

namespace Domain.Entities;

public partial class WalletBalance
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }

    public long BalanceVnd { get; set; }

    public WalletAccountType AccountType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
