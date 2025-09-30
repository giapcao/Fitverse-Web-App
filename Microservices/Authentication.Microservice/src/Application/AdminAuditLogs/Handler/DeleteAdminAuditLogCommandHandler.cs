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
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdminAuditLogCommandHandler(IRepository<AdminAuditLog> auditRepository, IUnitOfWork unitOfWork)
    {
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAdminAuditLogCommand request, CancellationToken ct)
    {
        var auditLog = (await _auditRepository.FindAsync(a => a.Id == request.Id, ct)).FirstOrDefault();
        if (auditLog is null)
        {
            return Result.Failure(new Error("AdminAudit.NotFound", $"Audit log {request.Id} was not found."));
        }

        _auditRepository.Delete(auditLog);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
