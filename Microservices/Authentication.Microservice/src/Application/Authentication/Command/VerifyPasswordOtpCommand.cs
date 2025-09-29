using Application.Abstractions.Messaging;
using MediatR;

namespace Application.Authentication.Command;

public sealed record VerifyPasswordOtpCommand(
    string Email,
    string Otp,
    string NewPassword) : ICommand<Unit>;