using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Domain.Persistence.Enums;

public enum SubscriptionEventType
{
    [PgName("SLOT_BOOKED")]
    [JsonStringEnumMemberName("SLOT_BOOKED")]
    SlotBooked,

    [PgName("SLOT_CANCELLED")]
    [JsonStringEnumMemberName("SLOT_CANCELLED")]
    SlotCancelled,

    [PgName("SLOT_COMPLETED")]
    [JsonStringEnumMemberName("SLOT_COMPLETED")]
    SlotCompleted,

    [PgName("SLOT_MISSING")]
    [JsonStringEnumMemberName("SLOT_MISSING")]
    SlotMissing
}
