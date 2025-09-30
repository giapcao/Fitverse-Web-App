using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Roles.Command;
using Domain.Entities;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Roles.Handler;

public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, RoleDto>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IRepository<Role> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var normalizedId = request.Id.Trim();
        var existing = await _roleRepository.FindAsync(r => r.Id == normalizedId, ct);
        if (existing.Any())
        {
            return Result.Failure<RoleDto>(new Error("Role.Exists", $"Role with id {normalizedId} already exists."));
        }

        var role = new Role
        {
            Id = normalizedId,
            DisplayName = request.DisplayName.Trim()
        };

        await _roleRepository.AddAsync(role, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new RoleDto(role.Id, role.DisplayName));
    }
}
