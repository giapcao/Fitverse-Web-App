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
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IRepository<Role> roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var normalizedId = request.Id.Trim();
        var role = (await _roleRepository.FindAsync(r => r.Id == normalizedId, ct)).FirstOrDefault();
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role with id {normalizedId} was not found."));
        }

        _roleRepository.Delete(role);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
