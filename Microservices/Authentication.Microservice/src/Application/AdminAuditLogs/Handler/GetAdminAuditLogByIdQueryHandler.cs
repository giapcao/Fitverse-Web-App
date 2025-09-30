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

public sealed class GetAdminAuditLogByIdQueryHandler : IQueryHandler<GetAdminAuditLogByIdQuery, AdminAuditLogDto>
{
    private readonly IRepository<AdminAuditLog> _auditRepository;

    public GetAdminAuditLogByIdQueryHandler(IRepository<AdminAuditLog> auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<Result<AdminAuditLogDto>> Handle(GetAdminAuditLogByIdQuery request, CancellationToken ct)
    {
        var auditLog = (await _auditRepository.FindAsync(a => a.Id == request.Id, ct)).FirstOrDefault();
        if (auditLog is null)
        {
            return Result.Failure<AdminAuditLogDto>(new Error("AdminAudit.NotFound", $"Audit log {request.Id} was not found."));
        }

        return Result.Success(AdminAuditLogMapping.ToDto(auditLog));
    }
}
