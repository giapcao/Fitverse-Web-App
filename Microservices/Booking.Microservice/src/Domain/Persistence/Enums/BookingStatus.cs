using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Domain.Persistence.Enums;

public enum BookingStatus
{
    [PgName("pending_payment")]
    [JsonStringEnumMemberName("pending_payment")]
    PendingPayment,

    [PgName("paid")]
    [JsonStringEnumMemberName("paid")]
    Paid,

    [PgName("confirmed")]
    [JsonStringEnumMemberName("confirmed")]
    Confirmed,

    [PgName("completed")]
    [JsonStringEnumMemberName("completed")]
    Completed,

    [PgName("cancelled_by_user")]
    [JsonStringEnumMemberName("cancelled_by_user")]
    CancelledByUser,

    [PgName("cancelled_by_coach")]
    [JsonStringEnumMemberName("cancelled_by_coach")]
    CancelledByCoach,

    [PgName("confirmed_by_user")]
    [JsonStringEnumMemberName("confirmed_by_user")]
    ConfirmedByUser,
    [PgName("confirmed_by_coach")]
    [JsonStringEnumMemberName("confirmed_by_coach")]
    ConfirmedByCoach,

    [PgName("no_show")]
    [JsonStringEnumMemberName("no_show")]
    NoShow,

    [PgName("disputed")]
    [JsonStringEnumMemberName("disputed")]
    Disputed
}
