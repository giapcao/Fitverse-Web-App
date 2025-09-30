using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.AdminAuditLogs.Command;
using Domain.Entities;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.AdminAuditLogs.Handler;

public sealed class ListAdminAuditLogsQueryHandler : IQueryHandler<ListAdminAuditLogsQuery, IEnumerable<AdminAuditLogDto>>
{
    private readonly IRepository<AdminAuditLog> _auditRepository;

    public ListAdminAuditLogsQueryHandler(IRepository<AdminAuditLog> auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<Result<IEnumerable<AdminAuditLogDto>>> Handle(ListAdminAuditLogsQuery request, CancellationToken ct)
    {
        var auditLogs = await _auditRepository.GetAllAsync(ct);
        var result = auditLogs.Select(AdminAuditLogMapping.ToDto);
        return Result.Success(result);
    }
}
