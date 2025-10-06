using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Roles.Command;
using Domain.Entities;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Roles.Handler;

public sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IRepository<Role> _roleRepository;

    public DeleteRoleCommandHandler(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = (await _roleRepository.FindAsync(r => r.Id == request.Id, ct)).FirstOrDefault();
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role with id {request.Id} was not found."));
        }

        _roleRepository.Delete(role);

        return Result.Success();
    }
}
