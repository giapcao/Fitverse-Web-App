using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Abstractions.Interface;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Common;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly Options.JwtOptions _opt;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _creds;

    public JwtTokenGenerator(
        IOptions<Options.JwtOptions> opt,
        SymmetricSecurityKey signingKey)
    {
        _opt = opt.Value;
        _signingKey = signingKey;
        _creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
    }

    public string CreateAccessToken(AppUser user, IEnumerable<string> roles)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new("sid", user.SecurityStamp)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,                                        
            expires: now.AddMinutes(_opt.AccessTokenMinutes),
            signingCredentials: _creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreatePurposeToken(Guid userId, string purpose, TimeSpan life)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("purpose", purpose)
        };

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(life),
            signingCredentials: _creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidatePurposeToken(string token, string expectedPurpose)
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidIssuer = _opt.Issuer,
            ValidAudience = _opt.Audience,
            IssuerSigningKey = _signingKey,             
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        }, out _);

        if (principal.FindFirst("purpose")?.Value != expectedPurpose)
            throw new SecurityTokenException("error token");

        return principal;
    }
}
