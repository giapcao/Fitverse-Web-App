using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.AdminAuditLogs.Command;
using Domain.Entities;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.AdminAuditLogs.Handler;

public sealed class DeleteAdminAuditLogCommandHandler : ICommandHandler<DeleteAdminAuditLogCommand>
{
    private readonly IRepository<AdminAuditLog> _auditRepository;

    public DeleteAdminAuditLogCommandHandler(IRepository<AdminAuditLog> auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<Result> Handle(DeleteAdminAuditLogCommand request, CancellationToken ct)
    {
        var auditLog = (await _auditRepository.FindAsync(a => a.Id == request.Id, ct)).FirstOrDefault();
        if (auditLog is null)
        {
            return Result.Failure(new Error("AdminAudit.NotFound", $"Audit log {request.Id} was not found."));
        }

        _auditRepository.Delete(auditLog);

        return Result.Success();
    }
}
