using Application.Abstractions.Messaging;
using MediatR;

namespace Application.Authentication.Command;

public sealed record RequestPasswordOtpCommand(string Email) : ICommand<Unit>;