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

public sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IRepository<Role> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var normalizedId = request.Id.Trim();
        var role = (await _roleRepository.FindAsync(r => r.Id == normalizedId, ct)).FirstOrDefault();
        if (role is null)
        {
            return Result.Failure<RoleDto>(new Error("Role.NotFound", $"Role with id {normalizedId} was not found."));
        }

        role.DisplayName = request.DisplayName.Trim();
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new RoleDto(role.Id, role.DisplayName));
    }
}
