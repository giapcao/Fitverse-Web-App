using System.Security.Claims;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, AuthDto.VerifyEmailResultDto>
{
    private readonly IJwtTokenGenerator _jwt;
    private readonly IAuthenticationRepository _authen;

    public VerifyEmailCommandHandler(IJwtTokenGenerator jwt, IAuthenticationRepository authen)
    {
        _jwt = jwt;
        _authen = authen;
    }

    public async Task<Result<AuthDto.VerifyEmailResultDto>> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var principal = _jwt.ValidatePurposeToken(request.Token, "email_confirm");
        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var userIdStr = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr)) throw new InvalidOperationException("Token is not valid");

        var userId = Guid.Parse(userIdStr);
        var user = await _authen.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("User is not valid");

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        _authen.Update(user);
        return Result.Success(new AuthDto.VerifyEmailResultDto(true));
    }
}

