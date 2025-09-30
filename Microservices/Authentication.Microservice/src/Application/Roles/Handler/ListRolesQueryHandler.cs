using System.Collections.Generic;
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

public sealed class ListRolesQueryHandler : IQueryHandler<ListRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IRepository<Role> _roleRepository;

    public ListRolesQueryHandler(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(ListRolesQuery request, CancellationToken ct)
    {
        var roles = await _roleRepository.GetAllAsync(ct);
        var result = roles.Select(r => new RoleDto(r.Id, r.DisplayName));
        return Result.Success(result);
    }
}
