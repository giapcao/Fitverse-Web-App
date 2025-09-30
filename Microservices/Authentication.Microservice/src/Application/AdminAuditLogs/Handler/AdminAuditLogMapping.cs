using System;
using Application.Features;
using Domain.Entities;

namespace Application.AdminAuditLogs.Handler;

internal static class AdminAuditLogMapping
{
    public static AdminAuditLogDto ToDto(AdminAuditLog auditLog) => new(
        auditLog.Id,
        auditLog.AdminId,
        auditLog.Action,
        auditLog.TargetType,
        auditLog.TargetId,
        auditLog.CreatedAt);
}
