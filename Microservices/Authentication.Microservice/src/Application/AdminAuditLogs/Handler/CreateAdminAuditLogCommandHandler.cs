using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.AdminAuditLogs.Command;
using Domain.Entities;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.AdminAuditLogs.Handler;

public sealed class CreateAdminAuditLogCommandHandler : ICommandHandler<CreateAdminAuditLogCommand, AdminAuditLogDto>
{
    private readonly IRepository<AdminAuditLog> _auditRepository;
    private readonly IAuthenticationRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdminAuditLogCommandHandler(
        IRepository<AdminAuditLog> auditRepository,
        IAuthenticationRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _auditRepository = auditRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminAuditLogDto>> Handle(CreateAdminAuditLogCommand request, CancellationToken ct)
    {
        var admin = await _userRepository.GetByIdAsync(request.AdminId, ct, asNoTracking: true);
        if (admin is null)
        {
            return Result.Failure<AdminAuditLogDto>(new Error("User.NotFound", $"Admin user {request.AdminId} was not found."));
        }

        var auditLog = new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminId = request.AdminId,
            Action = request.Action.Trim(),
            TargetType = request.TargetType.Trim(),
            TargetId = request.TargetId,
            CreatedAt = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(auditLog, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(AdminAuditLogMapping.ToDto(auditLog));
    }
}
