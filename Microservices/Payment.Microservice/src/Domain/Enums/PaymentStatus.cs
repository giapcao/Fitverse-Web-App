using NpgsqlTypes;

namespace Domain.Enums;

public enum PaymentStatus
{
    [PgName("initiated")]
    Initiated,

    [PgName("authorized")]
    Authorized,

    [PgName("captured")]
    Captured,

    [PgName("failed")]
    Failed,

    [PgName("refunded")]
    Refunded,

    [PgName("partial_refunded")]
    PartialRefunded
}
