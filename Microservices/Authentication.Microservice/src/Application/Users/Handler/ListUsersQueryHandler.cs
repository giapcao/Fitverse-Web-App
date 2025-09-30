using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Handler;

public sealed class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, IEnumerable<UserDto>>
{
    private readonly IAuthenticationRepository _userRepository;

    public ListUsersQueryHandler(IAuthenticationRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(ListUsersQuery request, CancellationToken ct)
    {
        var users = await _userRepository.GetAllDetailedAsync(ct);
        var result = users.Select(UserMapping.ToDto);
        return Result.Success(result);
    }
}
