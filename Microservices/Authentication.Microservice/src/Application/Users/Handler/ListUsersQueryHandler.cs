using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.Users.Handler;

public sealed class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, IEnumerable<UserDto>>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public ListUsersQueryHandler(IAuthenticationRepository userRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(ListUsersQuery request, CancellationToken ct)
    {
        var users = await _userRepository.GetAllDetailedAsync(ct);
        var mapped = users.Select(UserMapping.ToDto).ToArray();
        var result = await UserAvatarHelper.WithSignedAvatarsAsync(mapped, _fileStorageService, ct).ConfigureAwait(false);
        return Result.Success<IEnumerable<UserDto>>(result);
    }
}
