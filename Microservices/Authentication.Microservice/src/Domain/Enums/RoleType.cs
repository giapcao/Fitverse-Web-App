using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Enums;

public enum RoleType
{
    Customer,
    Coach,
    Admin,
    Support
}

public static class RoleTypeExtensions
{
    public static string GetDisplayName(this RoleType role) => role switch
    {
        RoleType.Customer => "Customer",
        RoleType.Coach => "Coach",
        RoleType.Admin => "Admin",
        RoleType.Support => "Support",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    public static Role ToEntity(this RoleType role) => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = role.GetDisplayName()
    };

    public static IReadOnlyCollection<Role> GetAll() => new[]
    {
        RoleType.Customer.ToEntity(),
        RoleType.Coach.ToEntity(),
        RoleType.Admin.ToEntity(),
        RoleType.Support.ToEntity()
    };
}

