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

    public CreateRoleCommandHandler(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var normalizedName = request.DisplayName.Trim();
        var normalizedNameLower = normalizedName.ToLowerInvariant();

        var existing = await _roleRepository.FindAsync(
            r => r.DisplayName != null && r.DisplayName.ToLower() == normalizedNameLower,
            ct);
        if (existing.Any())
        {
            return Result.Failure<RoleDto>(new Error("Role.Exists", $"Role with display name {normalizedName} already exists."));
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            DisplayName = normalizedName
        };

        await _roleRepository.AddAsync(role, ct);

        return Result.Success(new RoleDto(role.Id, role.DisplayName));
    }
}
