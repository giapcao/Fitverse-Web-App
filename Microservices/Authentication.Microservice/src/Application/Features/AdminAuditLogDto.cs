using System;

namespace Application.Features;

public sealed record AdminAuditLogDto(
    Guid Id,
    Guid AdminId,
    string Action,
    string TargetType,
    Guid? TargetId,
    DateTime CreatedAt);
