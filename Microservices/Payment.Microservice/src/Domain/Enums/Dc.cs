using NpgsqlTypes;

namespace Domain.Enums;

public enum Dc
{
    [PgName("DEBIT")]
    Debit,

    [PgName("CREDIT")]
    Credit
}
