using System;
using System.Linq;
using Application.Features;
using Domain.Entities;

namespace Application.Users.Handler;

internal static class UserMapping
{
    public static UserDto ToDto(AppUser user)
    {
        var roles = user.Roles?.Select(r => r.Id).ToArray() ?? Array.Empty<Guid>();

        return new UserDto(
            user.Id,
            user.Email,
            user.Phone,
            user.FullName,
            user.AvatarUrl,
            user.Gender,
            user.Birth,
            user.Description,
            user.HomeLat,
            user.HomeLng,
            user.IsActive,
            user.EmailConfirmed,
            user.CreatedAt,
            user.UpdatedAt,
            roles);
    }
}
