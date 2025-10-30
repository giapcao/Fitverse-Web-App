using NpgsqlTypes;

namespace Domain.Enums;

public enum WithdrawalRequestStatus
{
    [PgName("pending")]
    Pending,

    [PgName("approved")]
    Approved,

    [PgName("completed")]
    Completed,

    [PgName("rejected")]
    Rejected
}
