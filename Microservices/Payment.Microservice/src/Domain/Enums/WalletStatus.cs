using NpgsqlTypes;

namespace Domain.Enums;

public enum WalletStatus
{
    [PgName("active")]
    Active,

    [PgName("suspended")]
    Suspended,

    [PgName("closed")]
    Closed
}
