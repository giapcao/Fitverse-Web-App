using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.Entities;
using Domain.IRepositories;
using SharedLibrary.Common;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Storage;

namespace Application.Users.Handler;

public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IFileStorageService _fileStorageService;

    public CreateUserCommandHandler(
        IAuthenticationRepository userRepository,
        IRepository<Role> roleRepository,
        IPasswordHasher<AppUser> passwordHasher,
        IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _userRepository.FindByEmailAsync(normalizedEmail, ct, asNoTracking: true);
        if (existing is not null)
        {
            return Result.Failure<UserDto>(new Error("User.Exists", $"User with email {normalizedEmail} already exists."));
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Phone = request.Phone,
            FullName = request.FullName,
            Gender = request.Gender,
            Birth = request.Birth,
            Description = request.Description,
            HomeLat = request.HomeLat,
            HomeLng = request.HomeLng,
            IsActive = request.IsActive ?? true,
            EmailConfirmed = request.EmailConfirmed ?? false,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        if (request.RoleIds is not null && request.RoleIds.Any())
        {
            var distinctRoleIds = request.RoleIds.Where(id => id != Guid.Empty).Distinct().ToArray();
            if (distinctRoleIds.Length > 0)
            {
                var roles = await _roleRepository.FindAsync(r => distinctRoleIds.Contains(r.Id), ct);
                foreach (var role in roles)
                {
                    user.Roles.Add(role);
                }
            }
        }

        await _userRepository.AddAsync(user, ct);

        var dto = UserMapping.ToDto(user);
        dto = await UserAvatarHelper.WithSignedAvatarAsync(dto, _fileStorageService, ct).ConfigureAwait(false);
        return Result.Success(dto);
    }
}
