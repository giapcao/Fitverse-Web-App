using System.IdentityModel.Tokens.Jwt;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Features;
using Domain.IRepositories;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class RefreshCommandHandler : ICommandHandler<RefreshCommand, AuthDto.TokenPairDto>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IRefreshTokenStore _refresh;

    public RefreshCommandHandler(IAuthenticationRepository auth, IJwtTokenGenerator jwt, IRefreshTokenStore refresh)
    {
        _auth = auth; _jwt = jwt; _refresh = refresh;
    }

    public async Task<Result<AuthDto.TokenPairDto>> Handle(RefreshCommand request, CancellationToken ct)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(request.ExpiredAccessToken);
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                  ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub)) throw new UnauthorizedAccessException("Token is invalid");

        var userId = Guid.Parse(sub);
        var user = await _auth.GetByIdAsync(userId,ct) ?? throw new UnauthorizedAccessException("User is not exist");

        var (ok, _) = await _refresh.ValidateAsync(user.Id, request.RefreshToken, ct);
        if (!ok) throw new UnauthorizedAccessException("Refresh token is invalid");
        
        var roles = user.Roles.Select(r => r.Id);
        var newAccess = _jwt.CreateAccessToken(user, roles);
        var newRefresh = await _refresh.IssueAsync(user.Id, ct);

        return Result.Success(new AuthDto.TokenPairDto(newAccess, newRefresh));
    }
}