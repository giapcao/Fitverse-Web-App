using System.Security.Claims;
using Domain.Entities;

namespace Application.Abstractions.Interface;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(AppUser user, IEnumerable<string> roles);
    string CreatePurposeToken(Guid userId, string purpose, TimeSpan life);
    ClaimsPrincipal ValidatePurposeToken(string token, string expectedPurpose);
}