using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Common;
using Domain.IRepositories;
using Infrastructure.Common;
using MediatR;
using Microsoft.Extensions.Options;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class RequestPasswordOtpCommandHandler 
    : ICommandHandler<RequestPasswordOtpCommand, Unit>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IOtpStore _otp;
    private readonly IEmailSender _emailSender;
    private readonly IOptions<OtpOptions> _opt;

    public RequestPasswordOtpCommandHandler(
        IAuthenticationRepository auth,
        IOtpStore otp,
        IEmailSender emailSender,
        IOptions<OtpOptions> opt)
    {
        _auth = auth; _otp = otp; _emailSender = emailSender; _opt = opt;
    }

    public async Task<Result<Unit>> Handle(RequestPasswordOtpCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        
        var user = await _auth.FindByEmailAsync(email, ct);
        if (user is null) return Unit.Value;
        
        if (!await _otp.CanIssueAsync(email, ct))
            return Unit.Value;

        var otp = await _otp.IssueAsync(email, user.Id, TimeSpan.FromMinutes(_opt.Value.TtlMinutes), ct);
        
        var html = $@"
            <p>Xin chào,</p>
            <p>Mã OTP đặt lại mật khẩu của bạn là: <b>{otp}</b></p>
            <p>OTP có hiệu lực trong {_opt.Value.TtlMinutes} phút.</p>
            <p>Nếu không phải bạn yêu cầu, hãy bỏ qua email này.</p>";

        await _emailSender.SendAsync(email, "Mã OTP đặt lại mật khẩu", html, ct);

        return Unit.Value;
    }
}