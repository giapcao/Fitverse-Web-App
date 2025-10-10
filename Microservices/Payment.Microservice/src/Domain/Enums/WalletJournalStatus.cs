using NpgsqlTypes;

namespace Domain.Enums;

public enum WalletJournalStatus
{
    [PgName("pending")]
    Pending,

    [PgName("posted")]
    Posted,

    [PgName("cancelled")]
    Cancelled
}
