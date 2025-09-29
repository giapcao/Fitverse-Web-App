using System.Security.Claims;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Features;
using Domain.IRepositories;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, AuthDto.VerifyEmailResultDto>
{
    private readonly IJwtTokenGenerator _jwt;
    private readonly IAuthenticationRepository _authen;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(IJwtTokenGenerator jwt, IAuthenticationRepository authen,IUnitOfWork unitOfWork)
    {
        _jwt = jwt; _authen = authen;
        _unitOfWork = unitOfWork;
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
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(new AuthDto.VerifyEmailResultDto(true));
    }
}