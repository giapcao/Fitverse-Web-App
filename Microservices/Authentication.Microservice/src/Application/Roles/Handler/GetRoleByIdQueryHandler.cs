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

public sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleDto>
{
    private readonly IRepository<Role> _roleRepository;

    public GetRoleByIdQueryHandler(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken ct)
    {
        var role = (await _roleRepository.FindAsync(r => r.Id == request.Id, ct)).FirstOrDefault();
        if (role is null)
        {
            return Result.Failure<RoleDto>(new Error("Role.NotFound", $"Role with id {request.Id} was not found."));
        }

        return Result.Success(new RoleDto(role.Id, role.DisplayName));
    }
}
