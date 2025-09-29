using Application.Abstractions.Messaging;
using MediatR;

namespace Application.Authentication.Command;

public sealed record LogoutCommand() : ICommand<Unit>;
