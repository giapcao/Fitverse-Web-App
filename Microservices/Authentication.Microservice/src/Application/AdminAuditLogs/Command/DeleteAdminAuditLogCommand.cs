using System;
using Application.Abstractions.Messaging;

namespace Application.AdminAuditLogs.Command;

public sealed record DeleteAdminAuditLogCommand(Guid Id) : ICommand;
