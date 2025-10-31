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

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UpdateUserCommandHandler(IAuthenticationRepository userRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, ct);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id {request.Id} was not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.FindByEmailAsync(normalizedEmail, ct, asNoTracking: true);
                if (existing is not null && existing.Id != user.Id)
                {
                    return Result.Failure<UserDto>(new Error("User.EmailExists", $"Email {normalizedEmail} is already in use."));
                }

                user.Email = normalizedEmail;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            user.Phone = request.Phone.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            user.Gender = request.Gender.Trim();
        }

        if (request.Birth.HasValue)
        {
            user.Birth = request.Birth.Value;
        }

        if (request.Description is not null)
        {
            user.Description = request.Description.Trim();
        }

        if (request.HomeLat.HasValue)
        {
            user.HomeLat = request.HomeLat.Value;
        }

        if (request.HomeLng.HasValue)
        {
            user.HomeLng = request.HomeLng.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        _userRepository.Update(user);

        var dto = UserMapping.ToDto(user);
        dto = await UserAvatarHelper.WithSignedAvatarAsync(dto, _fileStorageService, ct).ConfigureAwait(false);
        return Result.Success(dto);
    }
}
