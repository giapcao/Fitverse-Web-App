using NpgsqlTypes;

namespace Domain.Enums;

public enum WalletJournalType
{
    [PgName("DEPOSIT")]
    Deposit,

    [PgName("HOLD")]
    Hold,

    [PgName("WITHDRAWAL_HOLD")]
    WithdrawalHold,

    [PgName("RELEASE")]
    Release,

    [PgName("CAPTURE")]
    Capture,

    [PgName("PAYOUT")]
    Payout,

    [PgName("REFUND")]
    Refund,

    [PgName("FEE")]
    Fee,

    [PgName("DISPUTE_FREEZE")]
    DisputeFreeze,

    [PgName("DISPUTE_RELEASE")]
    DisputeRelease
}
