using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Application.Features;
using Application.Roles.Command;
using Application.Users.Handler;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Roles.Handler;

public sealed class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand, UserDto>
{
    private readonly IAuthenticationRepository _userRepository;
    private readonly IRepository<Role> _roleRepository;

    public AssignRoleToUserCommandHandler(
        IAuthenticationRepository userRepository,
        IRepository<Role> roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<Result<UserDto>> Handle(AssignRoleToUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id {request.UserId} was not found."));
        }

        var roleName = request.Role.GetDisplayName();
        var normalizedRoleName = roleName.ToLowerInvariant();

        var role = (await _roleRepository.FindAsync(
            r => r.DisplayName != null && r.DisplayName.ToLower() == normalizedRoleName,
            ct)).FirstOrDefault();

        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                DisplayName = roleName
            };

            await _roleRepository.AddAsync(role, ct);
        }

        var hasRole = user.Roles.Any(r => string.Equals(r.DisplayName, roleName, StringComparison.OrdinalIgnoreCase));
        if (!hasRole)
        {
            user.Roles.Add(role);
            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);
        }

        return Result.Success(UserMapping.ToDto(user));
    }
}

