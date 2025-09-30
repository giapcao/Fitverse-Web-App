using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AdminAuditLogs.Command;

public sealed record GetAdminAuditLogByIdQuery(Guid Id) : IQuery<AdminAuditLogDto>;
