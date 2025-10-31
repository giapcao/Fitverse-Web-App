using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.Users.Handler;

public sealed class UpdateUserAvatarCommandHandler : ICommandHandler<UpdateUserAvatarCommand, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UpdateUserAvatarCommandHandler(
        IAuthenticationRepository userRepository,
        IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id {request.UserId} was not found."));
        }

        var file = request.File;
        if (file is null || file.Content.Length == 0)
        {
            return Result.Failure<UserDto>(new Error("User.InvalidAvatar", "Avatar file must be provided."));
        }

        if (UserAvatarHelper.TryGetStorageKey(user.AvatarUrl, out var existingKey))
        {
            await _fileStorageService.DeleteAsync(existingKey, cancellationToken).ConfigureAwait(false);
        }

        var directory = file.Directory ?? request.Directory ?? "user-avatar";
        var uploadResult = await _fileStorageService.UploadAsync(
            new FileUploadRequest(
                request.UserId,
                file.Content,
                file.FileName,
                file.ContentType,
                directory),
            cancellationToken).ConfigureAwait(false);

        user.AvatarUrl = uploadResult.Key;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);

        var dto = UserMapping.ToDto(user);
        dto = await UserAvatarHelper.WithSignedAvatarAsync(dto, _fileStorageService, cancellationToken).ConfigureAwait(false);
        return Result.Success(dto);
    }
}
