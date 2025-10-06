using NpgsqlTypes;

namespace Domain.Persistence.Enums;

public enum CoachMediaType
{
    [PgName("image")]
    Image,
    [PgName("video")]
    Video,
    [PgName("document")]
    Document
}
