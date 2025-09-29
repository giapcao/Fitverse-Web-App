using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Authentication.Command;

public sealed record VerifyEmailCommand(string Token) : ICommand<AuthDto.VerifyEmailResultDto>;