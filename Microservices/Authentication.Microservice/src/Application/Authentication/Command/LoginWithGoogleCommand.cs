using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Authentication.Command;

public sealed record LoginWithGoogleCommand(string Code, string State, string? RedirectUri) : ICommand<AuthDto.LoginResultDto>;
