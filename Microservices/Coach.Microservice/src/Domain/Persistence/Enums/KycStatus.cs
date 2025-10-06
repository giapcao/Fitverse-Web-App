using NpgsqlTypes;

namespace Domain.Persistence.Enums;

public enum KycStatus
{
    [PgName("pending")]
    Pending,
    [PgName("approved")]
    Approved,
    [PgName("rejected")]
    Rejected
}
