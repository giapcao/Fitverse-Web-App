using System.Collections.Generic;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AdminAuditLogs.Command;

public sealed record ListAdminAuditLogsQuery : IQuery<IEnumerable<AdminAuditLogDto>>;
