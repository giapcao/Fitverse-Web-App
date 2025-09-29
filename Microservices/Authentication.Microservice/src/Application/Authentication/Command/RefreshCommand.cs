using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.Authentication.Command;

public sealed record RefreshCommand(string ExpiredAccessToken, string RefreshToken) : ICommand<AuthDto.TokenPairDto>;
