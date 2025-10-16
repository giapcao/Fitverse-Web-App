using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Domain.Persistence.Enums;

public enum SubscriptionStatus
{
    [PgName("active")]
    [JsonStringEnumMemberName("active")]
    Active,

    [PgName("paused")]
    [JsonStringEnumMemberName("paused")]
    Paused,

    [PgName("canceled")]
    [JsonStringEnumMemberName("canceled")]
    Canceled,

    [PgName("expired")]
    [JsonStringEnumMemberName("expired")]
    Expired,

    [PgName("past_due")]
    [JsonStringEnumMemberName("past_due")]
    PastDue
}
