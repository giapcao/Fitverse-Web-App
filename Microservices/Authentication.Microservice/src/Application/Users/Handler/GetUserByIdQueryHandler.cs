using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Handler;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;

    public GetUserByIdQueryHandler(IAuthenticationRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetDetailedByIdAsync(request.Id, ct);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id {request.Id} was not found."));
        }

        return Result.Success(UserMapping.ToDto(user));
    }
}
