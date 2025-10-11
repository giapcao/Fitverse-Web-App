using NpgsqlTypes;

namespace Domain.Enums;

public enum WalletAccountType
{
    [PgName("AVAILABLE")]
    Available,

    [PgName("ESCROW")]
    Escrow,

    [PgName("FROZEN")]
    Frozen
}
