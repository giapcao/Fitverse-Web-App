using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Domain.Persistence.Enums;

public enum SlotStatus
{
    [PgName("open")]
    [JsonStringEnumMemberName("open")]
    Open,
    
    [PgName("pending")]
    [JsonStringEnumMemberName("pending")]
    Pending,

    [PgName("reserved")]
    [JsonStringEnumMemberName("reserved")]
    Reserved,

    [PgName("blocked")]
    [JsonStringEnumMemberName("blocked")]
    Blocked
}
