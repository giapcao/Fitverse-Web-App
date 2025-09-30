using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.AdminAuditLogs.Command;

public sealed record UpdateAdminAuditLogCommand(
    Guid Id,
    string? Action,
    string? TargetType,
    Guid? TargetId) : ICommand<AdminAuditLogDto>;
