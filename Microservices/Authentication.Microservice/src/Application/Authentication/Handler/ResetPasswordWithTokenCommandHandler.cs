using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Domain.Entities;
using Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class ResetPasswordWithTokenCommandHandler
    : ICommandHandler<ResetPasswordWithTokenCommand, Unit>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly IRefreshTokenStore _refreshStore;
    private readonly IJwtTokenGenerator _jwt; 

    public ResetPasswordWithTokenCommandHandler(
        IAuthenticationRepository auth,
        IPasswordHasher<AppUser> hasher,
        IRefreshTokenStore refreshStore,
        IJwtTokenGenerator jwt)
    {
        _auth = auth;
        _hasher = hasher;
        _refreshStore = refreshStore;
        _jwt = jwt;
    }

    public async Task<Result<Unit>> Handle(ResetPasswordWithTokenCommand request, CancellationToken ct)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = _jwt.ValidatePurposeToken(request.ResetToken, "pwd_reset_confirmed");
        }
        catch (SecurityTokenException)
        {
            return Unit.Value; 
        }

        var userIdStr =
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unit.Value;

        var user = await _auth.GetByIdAsync(userId, ct);
        if (user is null) return Unit.Value;

        var newHash = _hasher.HashPassword(user, request.NewPassword);
        await _auth.UpdatePasswordHashAsync(user.Id, newHash, ct);

        await _refreshStore.RevokeAsync(user.Id, ct);
        return Unit.Value;
    }
}