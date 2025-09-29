using System.Net;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Domain.IRepositories;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Unit>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IEmailSender _email;

    public ForgotPasswordCommandHandler(IAuthenticationRepository auth, IJwtTokenGenerator jwt, IEmailSender email)
    {
        _auth = auth; _jwt = jwt; _email = email;
    }

    public async Task<Result<Unit>> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _auth.FindByEmailAsync(email, ct);
        
        if (user is null) return Unit.Value;

        var token = _jwt.CreatePurposeToken(user.Id, "pwd_reset", TimeSpan.FromMinutes(30));
        var link = $"{request.ResetBaseUrl}?token={WebUtility.UrlEncode(token)}";

        await _email.SendAsync(email, "Reset password", $"Nhấn để đặt lại mật khẩu: {link}", ct);
        return Unit.Value;
    }
}