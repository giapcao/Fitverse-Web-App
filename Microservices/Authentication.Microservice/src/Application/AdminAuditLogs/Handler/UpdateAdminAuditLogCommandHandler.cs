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

public sealed class UpdateAdminAuditLogCommandHandler : ICommandHandler<UpdateAdminAuditLogCommand, AdminAuditLogDto>
{
    private readonly IRepository<AdminAuditLog> _auditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdminAuditLogCommandHandler(IRepository<AdminAuditLog> auditRepository, IUnitOfWork unitOfWork)
    {
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminAuditLogDto>> Handle(UpdateAdminAuditLogCommand request, CancellationToken ct)
    {
        var auditLog = (await _auditRepository.FindAsync(a => a.Id == request.Id, ct)).FirstOrDefault();
        if (auditLog is null)
        {
            return Result.Failure<AdminAuditLogDto>(new Error("AdminAudit.NotFound", $"Audit log {request.Id} was not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            auditLog.Action = request.Action.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.TargetType))
        {
            auditLog.TargetType = request.TargetType.Trim();
        }

        if (request.TargetId.HasValue)
        {
            auditLog.TargetId = request.TargetId;
        }

        _auditRepository.Update(auditLog);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(AdminAuditLogMapping.ToDto(auditLog));
    }
}
