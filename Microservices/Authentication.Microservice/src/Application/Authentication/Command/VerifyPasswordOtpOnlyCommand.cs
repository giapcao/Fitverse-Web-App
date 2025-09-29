using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Authentication.Command;

public sealed record VerifyPasswordOtpOnlyCommand(string Email, string Otp)
    : ICommand<AuthDto.VerifyOtpOkDto>;