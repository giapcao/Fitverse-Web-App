using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Common;
using Application.Features;
using Domain.IRepositories;
using Microsoft.Extensions.Options;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class VerifyPasswordOtpOnlyCommandHandler
    : ICommandHandler<VerifyPasswordOtpOnlyCommand, AuthDto.VerifyOtpOkDto>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IOtpStore _otp;
    private readonly IOptions<OtpOptions> _opt;
    private readonly IJwtTokenGenerator _jwt;

    public VerifyPasswordOtpOnlyCommandHandler(
        IAuthenticationRepository auth,
        IOtpStore otp,
        IOptions<OtpOptions> opt,
        IJwtTokenGenerator jwt)
    {
        _auth = auth;
        _otp = otp;
        _opt = opt;
        _jwt = jwt;
    }

    public async Task<Result<AuthDto.VerifyOtpOkDto>> Handle(VerifyPasswordOtpOnlyCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _auth.FindByEmailAsync(email, ct);

        var (ok, userIdInOtp) = await _otp.VerifyAndConsumeAsync(
            email, request.Otp, _opt.Value.MaxAttempts, ct);

        if (!(ok && user is not null && user.Id == userIdInOtp))
            return Result.Success(new AuthDto.VerifyOtpOkDto
            {
                ResetToken = string.Empty, ExpiresInSeconds = 0
            });

        var ttl = TimeSpan.FromMinutes(10);
        var resetToken = _jwt.CreatePurposeToken(user.Id, "pwd_reset_confirmed", ttl);

        return Result.Success(new AuthDto.VerifyOtpOkDto
        {
            ResetToken = resetToken,
            ExpiresInSeconds = (int)ttl.TotalSeconds
        });
    }
}