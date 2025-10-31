using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.Users.Handler;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetUserByIdQueryHandler(IAuthenticationRepository userRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetDetailedByIdAsync(request.Id, ct);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id {request.Id} was not found."));
        }

        var dto = UserMapping.ToDto(user);
        dto = await UserAvatarHelper.WithSignedAvatarAsync(dto, _fileStorageService, ct).ConfigureAwait(false);
        return Result.Success(dto);
    }
}
