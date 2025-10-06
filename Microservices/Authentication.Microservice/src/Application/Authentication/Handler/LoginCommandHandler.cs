using System.Linq;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Features;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthDto.LoginResultDto>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IRefreshTokenStore _refresh;
    private readonly IPasswordHasher<AppUser> _hasher;

    public LoginCommandHandler(
        IAuthenticationRepository auth,
        IJwtTokenGenerator jwt,
        IRefreshTokenStore refresh,
        IPasswordHasher<AppUser> hasher)
    {
        _auth = auth; _jwt = jwt; _refresh = refresh; _hasher = hasher;
    }

    public async Task<Result<AuthDto.LoginResultDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _auth.FindByEmailAsync(request.Email.Trim().ToLower(), ct);
        if (user is null || !user.IsActive || !user.EmailConfirmed)
            throw new UnauthorizedAccessException("Login failed");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Login failed");

        var roleNames = user.Roles.Select(r => r.DisplayName).ToArray();
        var roleDtos = user.Roles
            .Select(r => new RoleDto(r.Id, r.DisplayName))
            .ToArray();

        var access = _jwt.CreateAccessToken(user, roleNames);
        var refresh = await _refresh.IssueAsync(user.Id, ct);

        return new AuthDto.LoginResultDto(
            user.Id,
            user.Email!,
            user.FullName,
            roleDtos,
            new AuthDto.TokenPairDto(access, refresh));
    }
}

