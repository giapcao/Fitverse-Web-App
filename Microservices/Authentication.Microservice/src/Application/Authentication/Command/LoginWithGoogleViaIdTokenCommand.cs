using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Authentication.Command;

public sealed record LoginWithGoogleViaIdTokenCommand(string IdToken) : ICommand<AuthDto.LoginResultDto>;