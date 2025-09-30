using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Handler;

public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IAuthenticationRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, ct);
        if (user is null)
        {
            return Result.Failure(new Error("User.NotFound", $"User with id {request.Id} was not found."));
        }

        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
