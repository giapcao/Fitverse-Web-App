using System;
using System.Collections.Generic;
using System.Security.Claims;
using Domain.Entities;

namespace Application.Abstractions.Interface;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(AppUser user, IEnumerable<string> roleNames);
    string CreatePurposeToken(Guid userId, string purpose, TimeSpan life);
    ClaimsPrincipal ValidatePurposeToken(string token, string expectedPurpose);
}

