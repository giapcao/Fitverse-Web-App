using NpgsqlTypes;

namespace SharedLibrary.Contracts.Payments;

public enum Gateway
{
    [PgName("momo")]
    Momo,

    [PgName("zalopay")]
    Zalopay,

    [PgName("vnpay")]
    Vnpay,

    [PgName("payos")]
    Payos,

    [PgName("test")]
    Test
}
