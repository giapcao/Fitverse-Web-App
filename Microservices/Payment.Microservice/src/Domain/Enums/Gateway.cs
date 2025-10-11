using NpgsqlTypes;

namespace Domain.Enums;

public enum Gateway
{
    [PgName("momo")]
    Momo,

    [PgName("zalopay")]
    Zalopay,

    [PgName("vnpay")]
    Vnpay,

    [PgName("test")]
    Test
}
