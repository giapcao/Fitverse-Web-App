using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Authentication.Command;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthDto.LoginResultDto>;