using Application.Abstractions.Messaging;
using MediatR;

namespace Application.Authentication.Command;

public sealed record ForgotPasswordCommand(string Email, string ResetBaseUrl) : ICommand<Unit>;