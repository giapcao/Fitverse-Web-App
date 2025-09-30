using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Users.Command;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Users.Handler;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public UpdateUserCommandHandler(
        IAuthenticationRepository userRepository,
        IRepository<Role> roleRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
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

        user.Phone = request.Phone ?? user.Phone;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
        user.Gender = request.Gender ?? user.Gender;
        user.Birth = request.Birth ?? user.Birth;
        user.Description = request.Description ?? user.Description;
        user.HomeLat = request.HomeLat ?? user.HomeLat;
        user.HomeLng = request.HomeLng ?? user.HomeLng;

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        if (request.EmailConfirmed.HasValue)
        {
            user.EmailConfirmed = request.EmailConfirmed.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.SecurityStamp = Guid.NewGuid().ToString();
        }

        if (request.RoleIds is not null)
        {
            var distinctRoleIds = request.RoleIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            user.Roles.Clear();

            if (distinctRoleIds.Length > 0)
            {
                var roles = await _roleRepository.FindAsync(r => distinctRoleIds.Contains(r.Id), ct);
                foreach (var role in roles)
                {
                    user.Roles.Add(role);
                }
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(UserMapping.ToDto(user));
    }
}
