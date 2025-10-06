using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Handler;

public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IAuthenticationRepository _userRepository;

    public DeleteUserCommandHandler(
        IAuthenticationRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, ct);
        if (user is null)
        {
            return Result.Failure(new Error("User.NotFound", $"User with id {request.Id} was not found."));
        }

        _userRepository.Delete(user);

        return Result.Success();
    }
}
