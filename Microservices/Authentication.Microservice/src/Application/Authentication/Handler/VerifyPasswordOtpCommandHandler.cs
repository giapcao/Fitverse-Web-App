using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Common; // chứa OtpOptions (đổi theo namespace bạn dùng)
using Domain.Entities;
using Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class VerifyPasswordOtpCommandHandler
    : ICommandHandler<VerifyPasswordOtpCommand, Unit>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IOtpStore _otp;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly IOptions<OtpOptions> _opt;
    private readonly IRefreshTokenStore _refreshStore;

    public VerifyPasswordOtpCommandHandler(
        IAuthenticationRepository auth,
        IOtpStore otp,
        IPasswordHasher<AppUser> hasher,
        IOptions<OtpOptions> opt,
        IRefreshTokenStore refreshStore)
    {
        _auth = auth;
        _otp = otp;
        _hasher = hasher;
        _opt = opt;
        _refreshStore = refreshStore;
    }

    public async Task<Result<Unit>> Handle(VerifyPasswordOtpCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _auth.FindByEmailAsync(email, ct);
        var (ok, userIdInOtp) = await _otp.VerifyAndConsumeAsync(
            email, request.Otp, _opt.Value.MaxAttempts, ct);

        if (!(ok && user is not null && user.Id == userIdInOtp))
        {
            return Unit.Value;
        }

        var newHash = _hasher.HashPassword(user, request.NewPassword);

        await _auth.UpdatePasswordHashAsync(user.Id, newHash, ct);

        await _refreshStore.RevokeAsync(user.Id, ct);

        return Unit.Value;
    }
}